using Application;
using Infrastructure;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add application layer services (MediatR)
builder.Services.AddApplication();

// Add infrastructure layer services
builder.Services.AddInfrastructure();

// Add ASP.NET Core services
builder.Services.AddControllers();

// Add health checks
builder.Services.AddHealthChecks();

// Add swagger/openapi for development
builder.Services.AddOpenApi();

var app = builder.Build();

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
app.MapControllers();

app.Run();
