using Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
        // Register MediatR with handlers from the Application assembly and any loaded Infrastructure handlers.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Infrastructure");

            if (infrastructureAssembly is not null)
            {
                cfg.RegisterServicesFromAssembly(infrastructureAssembly);
            }
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Audit logging pipeline behavior — intercepts all commands (AC-03).
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));

        return services;
    }
}
