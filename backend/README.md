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
│   │   ├── Program.cs       # Application entry with DI setup
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   └── Controllers/
│   │       └── HealthController.cs  # Health check & ping endpoints
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
builder.Services.AddApplication();      // Register Application MediatR
builder.Services.AddInfrastructure();    // Register Infrastructure services
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
// Register data access, external services, etc.
```

## NuGet Dependencies

All packages use OSI-approved open-source licenses:

| Package | Version | License | Purpose |
|---------|---------|---------|---------|
| MediatR | 12.1.1 | Apache 2.0 | CQRS-Light pattern |
| Microsoft.Extensions.Hosting | 9.0.0 | MIT | Hosting infrastructure |

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

## Troubleshooting

### .NET SDK Version Mismatch

**Error**: `global.json specifies SDK version 9.0.0 which does not exist`

**Solution**: Install .NET 9 SDK or update `global.json` to an installed version.

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
