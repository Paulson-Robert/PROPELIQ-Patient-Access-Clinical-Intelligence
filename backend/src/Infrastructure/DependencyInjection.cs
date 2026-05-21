using Application.Interfaces;
using Infrastructure.Caching;
using Infrastructure.Data;
using Infrastructure.Data.Options;
using Infrastructure.Locking;
using Infrastructure.RateLimiting;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        services.AddScoped<IPatientDataDeletionService, PatientDataDeletionService>();

        // Booking services (US_018)
        services.AddScoped<ISlotSearchService, SlotSearchService>();
        services.AddScoped<IBookingConfirmationService, BookingConfirmationService>();
        services.AddScoped<IBookingPdfService, BookingPdfService>();

        // In-memory cache — required by RedisCacheService as fallback (AC-05).
        services.AddMemoryCache();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            // Use RedisConnectionFactory for Upstash TLS + retry configuration (AC-01).
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("RedisConnectionFactory");
                return RedisConnectionFactory.Create(redisConnectionString, logger);
            });

            // Primary cache: Redis with 15-minute sliding expiry + in-memory fallback (AC-02, AC-05).
            services.AddSingleton<ICacheService>(sp =>
                new RedisCacheService(
                    sp.GetRequiredService<IConnectionMultiplexer>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<ILogger<RedisCacheService>>()));

            // Distributed slot lock: SETNX with 30-second TTL (AC-03).
            services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
            services.AddSingleton<ISlotLockService, SlotLockService>();

            // Rate-limiting counter: sliding window per client + endpoint (AC-04).
            services.AddOptions<RateLimitOptions>()
                .BindConfiguration(RateLimitOptions.SectionName)
                .Validate(options => options.Limit > 0, "RateLimitOptions.Limit must be greater than 0.")
                .Validate(options => options.WindowSeconds > 0, "RateLimitOptions.WindowSeconds must be greater than 0.")
                .ValidateOnStart();
            services.AddSingleton<RedisSlidingWindowCounter>();
        }
        else
        {
            // Degraded mode: no Redis configured — serve entirely from in-memory cache (AC-05).
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            // Single-node in-memory slot lock (development / degraded mode).
            services.AddSingleton<ISlotLockService, InMemorySlotLockService>();
        }

        return services;
    }
}
