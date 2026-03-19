# .NET Aspire Integration Guide

## Overview

Your GoingMy microservices are now fully integrated with **.NET Aspire**, Microsoft's opinionated framework for orchestrating distributed .NET applications. Aspire streamlines cloud-native development with built-in service discovery, health checks, logging, and observability.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    GoingMy.AppHost                          │
│         (.NET Aspire Orchestration Layer)                   │
└──────────────────────────┬──────────────────────────────────┘
              ┌────────────┼────────────┐
              │            │            │
        ┌─────▼──┐   ┌─────▼──┐   ┌────▼───┐
        │PostgreSQL   │Auth API │   │Post API│
        │  (Aspire)   │ (Port 5001)│ (Port 5002)
        └──────────┘   └────────┘   └────────┘
              ▲              │            │
              └──────────────┴────────────┘
           Service Discovery & Health Monitoring
```

## Project Structure

```
src/
├── GoingMy.Services.slnx           ← ROOT SOLUTION (Master file)
├── GoingMy.AppHost/                ← Aspire Orchestrator
│   ├── AppHost.cs                  ← Defines services & resources
│   └── appsettings.json            ← Aspire config
├── GoingMy.ServiceDefaults/        ← Shared defaults
│   ├── Extensions.cs               ← Service registration
│   └── GoingMy.ServiceDefaults.csproj
├── GoingMy.AuthService/
│   └── src/GoingMy.Auth.API/       ← Uses ServiceDefaults
├── GoingMy.PostService/
│   └── src/GoingMy.Post.API/       ← Uses ServiceDefaults
└── README.md
```

## Running with Aspire

### Prerequisites
- .NET 10.0 SDK
- Docker or Podman (for PostgreSQL)
- Visual Studio 2024 or VS Code with C# extension

### Start Services with Aspire Dashboard

Run the AppHost to launch all services:

```powershell
cd src/GoingMy.AppHost
dotnet run
```

This will:
1. ✅ Start PostgreSQL container automatically
2. ✅ Launch Auth API on http://localhost:5001
3. ✅ Launch Post API on http://localhost:5002
4. ✅ Open Aspire Dashboard at http://localhost:18888

### Access Services

**Auth API (OpenAPI/Swagger)**
- Swagger UI: http://localhost:5001/swagger
- OpenAPI JSON: http://localhost:5001/openapi/v1.json
- Health Check: http://localhost:5001/health

**Post API (OpenAPI/Swagger)**
- Swagger UI: http://localhost:5002/swagger
- OpenAPI JSON: http://localhost:5002/openapi/v1.json
- Health Check: http://localhost:5002/health

**Aspire Dashboard**
- URL: http://localhost:18888
- View logs, metrics, traces
- Monitor service health
- Inspect resource dependencies

## Key Aspire Components

### 1. AppHost (GoingMy.AppHost)
The orchestration layer that:
- Defines all microservices as projects
- Provisiones PostgreSQL database
- Creates separate databases for each service
- Configures service-to-service communication
- Sets up health monitoring

**Key File: [AppHost.cs](GoingMy.AppHost/AppHost.cs)**
```csharp
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var authDb = postgres.AddDatabase("AuthDb");
builder.AddProject<Projects.GoingMy_Auth_API>("auth-api")
    .WithReference(authDb)
    .WaitFor(authDb);
```

### 2. ServiceDefaults
Shared utility library that provides:
- **Service Discovery** - Automatic DNS resolution of other services
- **Health Checks** - `/health` endpoint for Aspire monitoring
- **Observability** - OpenTelemetry integration for distributed tracing
- **Resilience** - Built-in retry and timeout patterns

**Key File: [Extensions.cs](GoingMy.ServiceDefaults/Extensions.cs)**
```csharp
// In each API's Program.cs:
builder.AddServiceDefaults();      // Register defaults
app.MapServiceDefaults();           // Map health checks
```

### 3. Service Integration

Each microservice API is configured for Aspire:

**Auth API Program.cs**
```csharp
using GoingMy.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();           // Add Aspire defaults
app.MapServiceDefaults();               // Map health endpoints
```

**Post API Program.cs** - Same pattern

## Service Discovery

Services automatically discover each other through Aspire:

```csharp
// In any service, inject HttpClient:
public class AuthService
{
    private readonly HttpClient _httpClient;
    
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task ValidateTokenAsync(string token)
    {
        // Automatically resolves "auth-api" through service discovery
        var response = await _httpClient.PostAsync(
            "http://auth-api/api/auth/validate",
            new StringContent(token)
        );
    }
}
```

## Health Checks

All services expose health endpoints for Aspire monitoring:

```
GET /health
```

Returns: `200 OK` with status information

**Used by:**
- Aspire Dashboard for real-time monitoring
- Service readiness checks
- Load balancer health probes

## Environment Configuration

### Development (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "GoingMy": "Debug"
    }
  }
}
```

### Production
Set `ASPNETCORE_ENVIRONMENT=Production` and provide secrets:
```powershell
dotnet user-secrets set "ConnectionStrings:AuthDb" "Server=...;"
```

## Distributed Tracing

OpenTelemetry is configured for automatic tracing:

**View traces in:**
- Aspire Dashboard > Traces tab
- Export via OTLP to external collectors

**Configuration:**
```csharp
// In ServiceDefaults/Extensions.cs
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });
```

## Database Management

### PostgreSQL in Aspire

The AppHost automatically:
1. Pulls official PostgreSQL image
2. Starts container on port 5432
3. Creates separate databases:
   - `AuthDb` for Auth Service
   - `PostDb` for Post Service
4. Provides pgAdmin for database management

**pgAdmin Access**
- URL: http://localhost:5050 (if configured)
- Manage databases and run queries

### Apply Migrations

```powershell
# For Auth Service
cd src/GoingMy.AuthService/src/GoingMy.Auth.Infrastructure
dotnet ef migrations add InitialCreate -s ../GoingMy.Auth.API
dotnet ef database update -s ../GoingMy.Auth.API

# For Post Service
cd src/GoingMy.PostService/src/GoingMy.Post.Infrastructure
dotnet ef migrations add InitialCreate -s ../GoingMy.Post.API
dotnet ef database update -s ../GoingMy.Post.API
```

## Common Commands

```powershell
# Navigate to src directory
cd src

# Build everything
dotnet build GoingMy.Services.slnx

# Run Aspire AppHost (orchestrates all services)
cd GoingMy.AppHost
dotnet run

# Test a specific service in isolation
cd GoingMy.AuthService
dotnet run --project src/GoingMy.Auth.API/GoingMy.Auth.API.csproj

# Run all tests
dotnet test GoingMy.Services.slnx

# Clean build artifacts
dotnet clean GoingMy.Services.slnx
```

## Troubleshooting

### Port Conflicts
If ports 5001, 5002, or 5432 are in use:
```powershell
# Find process on port
Get-NetTCPConnection -LocalPort 5001 | Select-Object OwningProcess
```

### PostgreSQL Container Issues
```powershell
# List containers
docker ps -a

# View logs
docker logs <container_id>

# Stop/remove container
docker stop <container_id>
docker rm <container_id>
```

### Service Discovery Not Working
1. Verify service names in AppHost match HTTP client URLs
2. Check health checks are returning 200 OK
3. View logs in Aspire Dashboard
4. Ensure services are using ServiceDefaults

### Database Connection Errors
1. Verify PostgreSQL is running: `docker ps`
2. Check connection string in appsettings
3. Confirm migrations have been applied
4. Review logs in Aspire Dashboard

## Monitoring & Logging

### Real-time Monitoring
1. Launch AppHost
2. Open http://localhost:18888
3. View:
   - Console logs
   - Distributed traces
   - Resource metrics
   - Service dependencies

### Log Levels
```csharp
// In appsettings.json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft": "Warning",
    "GoingMy": "Debug"
  }
}
```

### Structured Logging
Services use structured logging (ILogger):
```csharp
_logger.LogInformation("User {UserId} authenticated", userId);
_logger.LogError("Database error: {Error}", ex.Message);
```

## Next Steps

1. **Add Database Schemas**
   - Create DbContext for each service
   - Define entities and migrations

2. **Implement Service APIs**
   - Build domain models
   - Implement repositories
   - Create API controllers

3. **Add Service-to-Service Communication**
   - Use HttpClient for service calls
   - Implement circuit breakers
   - Add retry policies

4. **Production Deployment**
   - Build container images
   - Configure for Kubernetes/Docker Compose
   - Set up external OTLP collectors

5. **Advanced Aspire Features**
   - Custom components
   - Aspire manifests
   - CI/CD integration

## Documentation

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [OpenTelemetry](https://opentelemetry.io/)
- [Service Discovery](https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview)
- [Aspire Dashboard](https://github.com/dotnet/aspire/blob/main/src/Aspire.Dashboard/README.md)

## Key Files

| File | Purpose |
|------|---------|
| [GoingMy.Services.slnx](GoingMy.Services.slnx) | Root solution - use to build/run all |
| [GoingMy.AppHost/AppHost.cs](GoingMy.AppHost/AppHost.cs) | Service orchestration |
| [GoingMy.ServiceDefaults/Extensions.cs](GoingMy.ServiceDefaults/Extensions.cs) | Shared Aspire setup |
| [GoingMy.AuthService/src/GoingMy.Auth.API/Program.cs](GoingMy.AuthService/src/GoingMy.Auth.API/Program.cs) | Auth API configuration |
| [GoingMy.PostService/src/GoingMy.Post.API/Program.cs](GoingMy.PostService/src/GoingMy.Post.API/Program.cs) | Post API configuration |

## Support

For issues or questions:
1. Check Aspire Dashboard logs
2. Review skill system documentation
3. Consult team architecture guidelines
4. Review .skill-system/skills/knowledge files

---

**Created**: March 19, 2026  
**Framework**: .NET Aspire 13.1+  
**Services**: Auth, Post, PostgreSQL  
**Monitoring**: Aspire Dashboard + OpenTelemetry
