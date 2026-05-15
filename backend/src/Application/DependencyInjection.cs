using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Application;

/// <summary>
/// Dependency injection extension methods for the Application layer.
/// Registers application services including MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the dependency injection container.
    /// Configures MediatR pipeline with all handlers from this assembly.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR with handlers from the Application assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
