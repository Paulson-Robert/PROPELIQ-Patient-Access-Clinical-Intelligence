using API.Filters;
using Application;
using Hangfire;
using Infrastructure;
using Infrastructure.BackgroundJobs;
using Infrastructure.Data;
using Infrastructure.HealthChecks;
using Infrastructure.RateLimiting;
using Microsoft.Extensions.Logging;
using Serilog;

// Bootstrap logger captures startup errors before full Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from appsettings with request logging enrichment
builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

// Configure Sentry — graceful fallback if DSN is absent or misconfigured
var sentryDsn = builder.Configuration["Sentry:Dsn"];
if (string.IsNullOrWhiteSpace(sentryDsn))
{
    Log.Warning("Sentry DSN is not configured. Error tracking will be disabled. " +
                "Set Sentry__Dsn (or Sentry:Dsn in appsettings) to enable.");
}

builder.WebHost.UseSentry(options =>
{
    builder.Configuration.GetSection("Sentry").Bind(options);
    // Ensure a missing DSN does not prevent startup
    options.Dsn = sentryDsn ?? string.Empty;
});

// Add application layer services (MediatR)
builder.Services.AddApplication();

// Add infrastructure layer services (EF Core + Npgsql)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Hangfire background job processing with PostgreSQL storage
builder.Services.AddHangfireBackgroundJobs(builder.Configuration);

// Add ASP.NET Core services
builder.Services.AddControllers();

// Add health checks — PostgreSQL (built-in EF probe), Hangfire, Redis (AC-03)
var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "postgresql",
        timeout: TimeSpan.FromSeconds(5))
    .AddCheck<HangfireHealthCheck>(
        name: "hangfire",
        timeout: TimeSpan.FromSeconds(5));

// Redis health check is registered only when a connection string is present
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    healthChecksBuilder.AddCheck<RedisHealthCheck>(
        name: "redis",
        timeout: TimeSpan.FromSeconds(5));
}
else
{
    Log.Warning("Redis connection string is not configured. The Redis health check will be skipped.");
}

// Add swagger/openapi for development
builder.Services.AddOpenApi();

var app = builder.Build();

// Validate database connectivity on startup (fail fast with descriptive error)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
    try
    {
        if (!await db.Database.CanConnectAsync())
        {
            startupLogger.LogCritical(
                "Cannot connect to PostgreSQL. " +
                "Verify ConnectionStrings__DefaultConnection in appsettings and that the host is reachable.");
            throw new InvalidOperationException("Database connectivity check failed at startup.");
        }
        startupLogger.LogInformation("Database connection verified successfully.");
    }
    catch (Exception ex) when (ex is not InvalidOperationException)
    {
        startupLogger.LogCritical(ex,
            "Startup database check failed: {Message}. " +
            "Ensure pgcrypto is available and the Supabase connection string is correct.",
            ex.Message);
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Structured HTTP request logging via Serilog
app.UseSerilogRequestLogging();

// Redis sliding-window rate limiting — returns HTTP 429 + Retry-After when limits exceeded (AC-04).
app.UseRedisRateLimiting();

// Health check endpoint — detailed JSON response (AC-03)
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.UseHttpsRedirection();
app.UseAuthorization();

// Hangfire dashboard — authenticated Admin-only at /hangfire (unauthenticated → 401)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthFilter()],
});

app.MapControllers();

// Register recurring health-monitor job (every minute — verifies recurring job scheduling)
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate(
    "health-monitor",
    () => Console.WriteLine("[Hangfire] Health monitor: {0}", DateTime.UtcNow.ToString("O")),
    Cron.Minutely());

// Enqueue fire-and-forget startup verification job (AC-01)
var backgroundJobClient = app.Services.GetRequiredService<IBackgroundJobClient>();
backgroundJobClient.Enqueue(() => Console.WriteLine("[Hangfire] Startup verification job executed."));

app.Run();

}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
