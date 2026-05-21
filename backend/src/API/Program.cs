using API.Filters;
using API.Authorization;
using API.Middleware;
using Application;
using Hangfire;
using Infrastructure;
using Infrastructure.Auth;
using Infrastructure.BackgroundJobs;
using Infrastructure.Data;
using Infrastructure.Data.Seed;
using Infrastructure.HealthChecks;
using Infrastructure.Jobs;
using Infrastructure.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
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

// Add authentication services (JWT + OpenIddict + OAuth handlers)
builder.Services.AddOpenIddictAuthentication(builder.Configuration);
builder.Services.AddRoleAuthorizationPolicies();

// Add Hangfire background job processing with PostgreSQL storage
builder.Services.AddHangfireBackgroundJobs(builder.Configuration);

// Add ASP.NET Core services
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:5173"];

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add health checks — PostgreSQL (built-in EF probe), Hangfire, Redis (AC-03)
var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "postgresql")
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

        if (app.Environment.IsDevelopment())
        {
            await DatabaseSeeder.SeedAsync(db, startupLogger);
        }
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
// Exempt infrastructure endpoints so health probes and operational tooling remain available under load.
app.UseWhen(
    context =>
        !context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase) &&
        !context.Request.Path.StartsWithSegments("/hangfire", StringComparison.OrdinalIgnoreCase) &&
        !context.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase),
    branch => branch.UseRedisRateLimiting());

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
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseSessionSlidingExpiry();
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

// Register Google Calendar sync recurring job — every 5 minutes (US_025 AC-05)
recurringJobManager.AddOrUpdate<GoogleCalendarSyncJob>(
    "google-calendar-sync",
    job => job.ExecuteAsync(),
    "*/5 * * * *");

// Register Outlook Calendar sync recurring job — every 5 minutes (US_026 AC-05)
recurringJobManager.AddOrUpdate<OutlookCalendarSyncJob>(
    "outlook-calendar-sync",
    job => job.ExecuteAsync(),
    "*/5 * * * *");

// Register appointment reminder scheduling job — every 15 minutes (US_027)
recurringJobManager.AddOrUpdate<ScheduleRemindersJob>(
    "schedule-appointment-reminders",
    job => job.ExecuteAsync(),
    "*/15 * * * *");

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
