# .NET Service Template

This is a template for creating a new .NET microservice in GoingMy.

## How to Use This Template

1. Copy this entire directory
2. Find and replace `{ServiceName}` with your service name (e.g., Post, Notification)
3. Follow the files in order:
   - Domain (entities, interfaces)
   - Application (services, DTOs)
   - Infrastructure (data access, repositories)
   - API (controllers, middleware)
4. Update your connections strings
5. Run migrations

## Directory Structure

```
GoingMy.{ServiceName}Service/
├── src/
│   ├── GoingMy.{ServiceName}.API/
│   ├── GoingMy.{ServiceName}.Domain/
│   ├── GoingMy.{ServiceName}.Application/
│   └── GoingMy.{ServiceName}.Infrastructure/
└── tests/
    ├── GoingMy.{ServiceName}.Tests/
    └── GoingMy.{ServiceName}.IntegrationTests/
```

## Quick Start

```bash
# Create solution
dotnet new sln -n GoingMy.{ServiceName}Service

# Create projects
dotnet new webapi -n GoingMy.{ServiceName}.API
dotnet new classlib -n GoingMy.{ServiceName}.Domain
dotnet new classlib -n GoingMy.{ServiceName}.Application
dotnet new classlib -n GoingMy.{ServiceName}.Infrastructure
dotnet new xunit -n GoingMy.{ServiceName}.Tests

# Add to solution
dotnet sln GoingMy.{ServiceName}Service.sln add **/*.csproj

# Restore and build
dotnet restore
dotnet build
```

## Files Included

- **Entity.cs** - Domain entity template
- **I{ServiceName}Service.cs** - Service interface
- **{ServiceName}Service.cs** - Implementation
- **{ServiceName}Controller.cs** - API controller
- **{ServiceName}DbContext.cs** - Database context
- **I{ServiceName}Repository.cs** - Repository interface
- **{ServiceName}Repository.cs** - Repository implementation
- **{ServiceName}Dto.cs** - Data transfer objects
- **{ServiceName}ServiceTests.cs** - Unit tests

## Next Steps

1. Define your domain entities
2. Create application services
3. Implement data access layer
4. Create API endpoints
5. Write tests
6. Add to docker-compose.yml
7. Create migrations

## References

- [Microservices Architecture](../skills/knowledge/microservices-architecture.md)
- [.NET Coding Standards](../skills/knowledge/dotnet-coding-standards.md)
- [Scaffolding Guide](../skills/scaffolding/scaffold-netcore-microservice.md)
