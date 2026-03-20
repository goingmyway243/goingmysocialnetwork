# GoingMy Microservices - Scaffolded Services

## Overview

This directory contains the scaffolded .NET Core microservices for the GoingMy social network platform. The services are structured using a clean architecture pattern with clear separation of concerns.

## Project Structure

```
src/
├── GoingMy.AuthService/          # Authentication and authorization service
│   ├── GoingMy.AuthService.slnx  # Solution file
│   ├── src/
│   │   ├── GoingMy.Auth.Domain/          # Domain models and entities
│   │   ├── GoingMy.Auth.Application/     # Application services and DTOs
│   │   ├── GoingMy.Auth.Infrastructure/  # Data access and external services
│   │   └── GoingMy.Auth.API/             # REST API controllers
│   └── tests/
│       ├── GoingMy.Auth.Tests/                    # Unit tests
│       └── GoingMy.Auth.IntegrationTests/        # Integration tests
│
├── GoingMy.PostService/           # Post management service
│   ├── GoingMy.PostService.slnx  # Solution file
│   ├── src/
│   │   ├── GoingMy.Post.Domain/          # Domain models and entities
│   │   ├── GoingMy.Post.Application/     # Application services and DTOs
│   │   ├── GoingMy.Post.Infrastructure/  # Data access and external services
│   │   └── GoingMy.Post.API/             # REST API controllers
│   └── tests/
│       ├── GoingMy.Post.Tests/                    # Unit tests
│       └── GoingMy.Post.IntegrationTests/        # Integration tests
│
└── GoingMy.ServiceDefaults/        # Shared service defaults (TBD)
```

## Architecture Layers

Each service follows the clean architecture pattern with four main layers:

### 1. Domain Layer (`{ServiceName}.Domain`)
- **Purpose**: Contains core business logic and rules
- **Contents**:
  - Entity classes
  - Value objects
  - Domain interfaces
  - Business exceptions
- **Dependencies**: None (self-contained)

### 2. Application Layer (`{ServiceName}.Application`)
- **Purpose**: Orchestrates domain logic and handles cross-cutting concerns
- **Contents**:
  - Application services
  - Data Transfer Objects (DTOs)
  - Validators (FluentValidation)
  - MediatR commands/queries (optional)
- **Dependencies**: Domain layer only

### 3. Infrastructure Layer (`{ServiceName}.Infrastructure`)
- **Purpose**: Handles technical details and external integrations
- **Contents**:
  - Entity Framework DbContext
  - Repository implementations
  - Database migrations
  - External service integrations
  - Logging and caching
- **Dependencies**: Domain and Application layers

### 4. API Layer (`{ServiceName}.API`)
- **Purpose**: Exposes REST endpoints to external consumers
- **Contents**:
  - Controllers
  - Program.cs (configuration and setup)
  - appsettings.json
  - Dockerfile
- **Dependencies**: All other layers

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- PostgreSQL (for database)
- Docker (optional, for containerization)

### Build Instructions

**Build a specific service:**
```powershell
cd src\GoingMy.AuthService
dotnet build GoingMy.AuthService.slnx
```

**Run tests:**
```powershell
cd src\GoingMy.AuthService
dotnet test GoingMy.AuthService.slnx
```

**Run the API:**
```powershell
cd src\GoingMy.AuthService\src\GoingMy.Auth.API
dotnet run
```

### Add NuGet Dependencies

Add required packages to the API project:

```powershell
cd src\GoingMy.AuthService\src\GoingMy.Auth.API

# Core packages
dotnet add package Microsoft.EntityFrameworkCore.PostgreSQL
dotnet add package Serilog
dotnet add package FluentValidation
dotnet add package MediatR

# Testing packages (in test projects)
dotnet add package Moq
```

## Service Responsibilities

### Auth Service (`GoingMy.AuthService`)
- User registration and login
- JWT token generation and validation
- User profile management
- Password management
- OAuth/SSO integration (future)

**Default API Port**: 5001

### Post Service (`GoingMy.PostService`)
- Create, read, update, delete posts
- Post comments management
- Like/reaction handling
- Post feed management
- Media attachment support

**Default API Port**: 5002

## Next Steps

1. **Add Domain Models**: Implement entity classes in the Domain layer
   - Example: `Post`, `Comment`, `User` entities

2. **Configure Database**: Set up DbContext in Infrastructure layer
   - Create migrations
   - Configure connection strings in appsettings.json

3. **Implement Repositories**: Create repository pattern for data access
   - Generic repository base class
   - Service-specific repositories

4. **Build Application Services**: Create business logic handlers
   - Command handlers (MediatR)
   - Query handlers
   - Service classes

5. **Create API Endpoints**: Add controllers with REST endpoints
   - Request/response validation
   - Error handling
   - Documentation (Swagger/OpenAPI)

6. **Write Tests**: Implement comprehensive test coverage
   - Unit tests in `Tests` project
   - Integration tests in `IntegrationTests` project

7. **Docker Preparation**: Create and test Dockerfile
   - Multi-stage build
   - Health checks
   - Environment configuration

## Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "{ServiceName}Db": "Server=localhost;Port=5432;Database=goingmy_{service};User Id=postgres;Password=password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

### Environment-Specific Settings
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production configuration

## Verification Checklist

- [ ] Service builds without errors
- [ ] All tests pass (`dotnet test`)
- [ ] API starts successfully (`dotnet run`)
- [ ] Swagger/API documentation is accessible
- [ ] Database migrations can be applied
- [ ] Service can be containerized (Docker builds)

## Common Commands

```powershell
# Navigate to service
cd src\GoingMy.AuthService

# Build solution
dotnet build GoingMy.AuthService.slnx

# Run tests
dotnet test GoingMy.AuthService.slnx

# Watch for changes and run
dotnet watch run --project src/GoingMy.Auth.API

# Add migration
cd src/GoingMy.Auth.Infrastructure
dotnet ef migrations add InitialCreate -s ../GoingMy.Auth.API

# Apply migrations
cd src/GoingMy.Auth.Infrastructure
dotnet ef database update -s ../GoingMy.Auth.API

# Check NuGet packages
dotnet list package --outdated
```

## Resources

- **.NET Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/
- **Clean Architecture**: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **Microservices Patterns**: https://microservices.io/patterns/

## Support

For questions or issues:
1. Check the skill system documentation in `.instruction.md/`
2. Review the relevant service's code and tests
3. Consult the team lead or architecture team

---

**Created**: March 19, 2026
**Technology**: .NET 10.0, PostgreSQL, xUnit
**Architecture Pattern**: Clean Architecture with Microservices
