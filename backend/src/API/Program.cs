using API.Filters;
using Application;
using Hangfire;
using Infrastructure;
using Infrastructure.BackgroundJobs;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add application layer services (MediatR)
builder.Services.AddApplication();

// Add infrastructure layer services (EF Core + Npgsql)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Hangfire background job processing with PostgreSQL storage
builder.Services.AddHangfireBackgroundJobs(builder.Configuration);

// Add ASP.NET Core services
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks();

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

// Health check endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
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
