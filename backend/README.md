# Backend API — Clean Architecture Scaffold

## Overview

ASP.NET Core 9 backend solution with Clean Architecture layers and MediatR CQRS-Light pattern for consistent, testable, and maintainable feature development.

## Project Structure

```
backend/
├── Backend.sln              # Solution file
├── global.json              # .NET 9 SDK version constraint
├── nuget.config             # NuGet package source configuration
├── src/
│   ├── API/                 # Presentation layer (ASP.NET Core Web API)
│   │   ├── API.csproj
│   │   ├── Program.cs       # Application entry with DI setup and health endpoint mapping
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   └── Controllers/
│   │       └── PingController.cs    # Ping endpoint controller
│   ├── Application/         # Business logic layer (MediatR handlers)
│   │   ├── Application.csproj
│   │   ├── DependencyInjection.cs   # DI registration for MediatR
│   │   ├── Commands/
│   │   │   └── PingCommand.cs       # Sample CQRS command
│   │   └── Handlers/
│   │       └── PingCommandHandler.cs # Sample command handler
│   ├── Domain/              # Domain entities layer
│   │   └── Domain.csproj
│   └── Infrastructure/      # Data access & external services layer
│       ├── Infrastructure.csproj
│       └── DependencyInjection.cs   # DI registration for infrastructure
```

## Dependency Flow

```
API (Presentation)
  ↓ references
Application (Business Logic)  Infrastructure (Data Access)
  ↓ references                  ↓ references
Domain (Entities)
```

This layered approach ensures:
- **Separation of Concerns**: Each layer has a single responsibility
- **Testability**: Business logic is decoupled from infrastructure
- **Maintainability**: Clear dependency boundaries
- **Scalability**: Easy to add features without affecting core logic

## Getting Started

### Prerequisites

- .NET 9 SDK (checked via `global.json`)
- Visual Studio 2022, VS Code with C# Dev Kit, or Rider

### Build

```bash
cd backend
dotnet build
```

**Expected output:**
```
Build succeeded. 0 Warning(s)
```

### Run

```bash
cd backend/src/API
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
...
```

### Health Check

Once the API is running, test the health endpoint:

```bash
curl http://localhost:5000/health
```

**Expected response:**
```json
{"status":"Healthy"}
```

### Test MediatR Pipeline

Verify the MediatR CQRS-Light pipeline with the sample PingCommand:

```bash
curl -X POST http://localhost:5000/api/health/ping
```

**Expected response:**
```json
{"message":"pong"}
```

## Architecture Patterns

### Clean Architecture

The solution follows **Clean Architecture** principles:

- **API Layer**: HTTP controllers, routing, input validation
- **Application Layer**: Business logic, MediatR commands/queries, handlers
- **Domain Layer**: Core entities, value objects, domain logic
- **Infrastructure Layer**: Data access, external services, framework abstractions

### MediatR CQRS-Light

The Application layer uses **MediatR** for CQRS-Light pattern:

- **Commands**: Modify state (e.g., `PingCommand`)
- **Queries**: Retrieve state (future queries follow the same pattern)
- **Handlers**: Process commands/queries with business logic
- **Pipeline**: Pre/post processing behaviors for cross-cutting concerns

**Benefits:**
- Decouples command invocation from handling
- Enables easy addition of pipeline behaviors (logging, validation, error handling)
- Simplifies testing through dependency injection

## Dependency Injection

All layers use ASP.NET Core's built-in `IServiceCollection` for DI:

### API (Program.cs)

```csharp
builder.Services.AddApplication();                       // Register Application MediatR
builder.Services.AddInfrastructure(builder.Configuration); // Register Infrastructure services
```

### Application (DependencyInjection.cs)

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
});
```

### Infrastructure (DependencyInjection.cs)

```csharp
// Register EF Core DbContext with Npgsql provider
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.SetPostgresVersion(16, 0);
        npgsql.EnableRetryOnFailure(maxRetryCount: 3, ...);
    }));
```

## NuGet Dependencies

All packages use OSI-approved open-source licenses:

| Package | Version | License | Purpose |
|---------|---------|---------|---------|
| MediatR | 12.1.1 | Apache 2.0 | CQRS-Light pattern |
| Microsoft.Extensions.Hosting | 9.0.0 | MIT | Hosting infrastructure || Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | PostgreSQL | PostgreSQL EF Core provider |
| Microsoft.EntityFrameworkCore | 9.0.5 | MIT | ORM core |
| Microsoft.EntityFrameworkCore.Design | 9.0.5 | MIT | Migration tooling |
Run `dotnet list package --format json` to audit licenses.

## Extending the Solution

### Adding a New Feature

1. **Domain**: Define entities/value objects if needed
2. **Application**: Create command/query + handler
3. **Infrastructure**: Add data access if needed
4. **API**: Add controller endpoint

### Adding a Pipeline Behavior

```csharp
// Application/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Pre-processing
        var response = await next();
        // Post-processing
        return response;
    }
}

// Register in Application/DependencyInjection.cs
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

## Database

### Connection

The API requires a PostgreSQL 16 database (Supabase free tier). Set the connection string in `src/API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.<project-ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

For Supabase connection pooler (recommended for serverless / high concurrency), use port **6543** with the pooler username format `postgres.<project-ref>`.

### Hybrid Migration Strategy (AC-03)

This project enforces a **hybrid migration strategy**: EF Core C# migrations are the single source of truth for schema definitions, and SQL scripts are exported from those migrations for production deployment review.

**Rule:** Never apply EF Core migrations directly to production. Always export and review the SQL script first.

#### Development workflow

```bash
# 1. Add a new migration after model changes
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure \
  --startup-project src/API

# 2. Apply migration to local/dev database
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/API
```

#### Production workflow

```bash
# 1. Export migration to SQL script for DBA review
dotnet ef migrations script \
  --project src/Infrastructure \
  --startup-project src/API \
  --idempotent \
  --output migrations/$(date +%Y%m%d)_pending.sql

# 2. Review the generated SQL, then apply via psql or Supabase SQL editor
psql "$CONNECTION_STRING" -f migrations/<date>_pending.sql
```

#### pgcrypto extension

The initial migration enables `pgcrypto` for PHI encryption (DR-001). This is declared in `ApplicationDbContext.OnModelCreating` via `modelBuilder.HasPostgresExtension("pgcrypto")` and emitted as `CREATE EXTENSION IF NOT EXISTS pgcrypto` in the migration SQL.

Verify pgcrypto is available on your Supabase instance:

```sql
SELECT * FROM pg_extension WHERE extname = 'pgcrypto';
```

If missing, enable it via the Supabase dashboard → Database → Extensions.

## Troubleshooting

### .NET SDK Version Mismatch

**Error**: `global.json specifies SDK version 9.0.100 which does not exist`

**Solution**: Install the .NET 9 SDK version pinned in `global.json` or update `global.json` to a version installed on your machine.

### Port Already in Use

**Error**: `Address already in use`

**Solution**: Change the port in `launchSettings.json` or kill the process using the current port.

### NuGet Restore Failure

**Error**: `Unable to resolve package source`

**Solution**: Check `nuget.config` or configure proxy settings:

```bash
dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
```

## References

- [ASP.NET Core 9 Documentation](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-9.0)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)
- [Clean Architecture Pattern](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
