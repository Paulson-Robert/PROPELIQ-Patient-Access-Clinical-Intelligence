namespace API.Configuration;

/// <summary>
/// Extension methods for configuring HSTS and security response headers (AC-02, AC-04).
/// </summary>
public static class SecurityHeadersConfig
{
    /// <summary>
    /// Registers HSTS options with a 1-year max-age, including subdomains (AC-04).
    /// Call from <c>WebApplicationBuilder.Services</c> before <c>Build()</c>.
    /// </summary>
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            // AC-04: 1-year max-age
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            // Preload is intentionally false until the domain is submitted to the HSTS preload list.
            options.Preload = false;
        });

        return services;
    }

    /// <summary>
    /// Adds HSTS and defence-in-depth security response headers to the pipeline.
    /// HSTS is skipped in Development to avoid browser pin issues during local work (AC-04).
    /// Call before <c>UseHttpsRedirection</c>.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
            await next().ConfigureAwait(false);
        });

        return app;
    }
}
