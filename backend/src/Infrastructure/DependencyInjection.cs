using Infrastructure.Data;
using Infrastructure.Data.Options;
using Infrastructure.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fail fast at startup if the PHI encryption key is absent or wrong length.
        // Design-time tools (dotnet ef) skip IHost.StartAsync so migrations still work.
        services.AddSingleton<IValidateOptions<PhiEncryptionOptions>, PhiEncryptionOptionsValidator>();
        services.AddOptions<PhiEncryptionOptions>()
            .BindConfiguration(PhiEncryptionOptions.SectionName)
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Set ConnectionStrings__DefaultConnection in appsettings or environment variables.");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.SetPostgresVersion(16, 0);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }));

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString));
        }

        return services;
    }
}
