# GoingMy Social Network - Full Stack Application

## Overview

This directory contains the complete GoingMy social network platform with both backend microservices and frontend web application. The backend services are structured using a clean architecture pattern with clear separation of concerns, while the frontend is built with Angular 20 and PrimeNG for a modern, glassmorphic UI experience.

## Project Structure

```
src/
├── GoingMy.ApiGateway/            # YARP reverse proxy gateway (single entry point)
│   ├── Program.cs                # YARP + OpenIddict + rate limiting
│   ├── appsettings.json          # YARP route & cluster configuration
│   ├── appsettings.Development.json
│   ├── Properties/launchSettings.json
│   └── GoingMy.ApiGateway.csproj
│
├── GoingMy.Web/                   # Angular 20 frontend application
│   ├── src/
│   │   ├── app/                  # Angular components and modules
│   │   ├── assets/               # Static assets
│   │   ├── styles.css            # Global styles with glassmorphism theme
│   │   └── main.ts               # Application entry point
│   ├── package.json              # npm dependencies
│   ├── angular.json              # Angular CLI configuration
│   └── tsconfig.json             # TypeScript configuration
│
├── GoingMy.AuthService/          # Authentication and authorization service
│   ├── GoingMy.AuthService.slnx  # Solution file
│   ├── src/
│   │   └── GoingMy.Auth.API/             # REST API (identity only — no profile fields)
│   └── tests/
│       ├── GoingMy.Auth.Tests/           # Unit tests
│       └── GoingMy.Auth.IntegrationTests/# Integration tests
│
├── GoingMy.UserService/          # User profile, followers, avatar, cover service
│   ├── GoingMy.UserService.slnx  # Solution file
│   ├── src/
│   │   ├── GoingMy.User.Domain/          # UserProfile, UserFollow entities & repo interfaces
│   │   ├── GoingMy.User.Application/     # CQRS commands, queries, DTOs (MediatR)
│   │   ├── GoingMy.User.Infrastructure/  # EF Core / PostgreSQL repositories & migrations
│   │   └── GoingMy.User.API/             # REST API controllers
│   └── tests/
│       ├── GoingMy.User.Tests/           # Unit tests
│       └── GoingMy.User.IntegrationTests/# Integration tests
│
├── GoingMy.PostService/          # Post management service
│   ├── GoingMy.PostService.slnx  # Solution file
│   ├── src/
│   │   ├── GoingMy.Post.Domain/          # Domain models and entities
│   │   ├── GoingMy.Post.Application/     # Application services and DTOs
│   │   ├── GoingMy.Post.Infrastructure/  # Data access and external services
│   │   └── GoingMy.Post.API/             # REST API controllers
│   └── tests/
│       ├── GoingMy.Post.Tests/           # Unit tests
│       └── GoingMy.Post.IntegrationTests/# Integration tests
│
├── GoingMy.ChatService/          # Real-time chat and messaging service
│   ├── GoingMy.ChatService.slnx  # Solution file
│   ├── src/
│   │   ├── GoingMy.Chat.Domain/          # Domain models and entities
│   │   ├── GoingMy.Chat.Application/     # CQRS commands, queries, DTOs
│   │   ├── GoingMy.Chat.Infrastructure/  # MongoDB repositories and context
│   │   └── GoingMy.Chat.API/             # REST controllers + SignalR hub
│   └── tests/
│       ├── GoingMy.Chat.Tests/           # Unit tests
│       └── GoingMy.Chat.IntegrationTests/# Integration tests
│
├── GoingMy.AppHost/              # .NET Aspire orchestration host
├── GoingMy.ServiceDefaults/      # Shared service defaults and extensions
├── GoingMy.Shared/               # Shared utilities and configurations
│
└── GoingMy.Social.slnx         # Main solution file (includes all projects)
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

## Frontend Architecture

### Web Application (`GoingMy.Web`)

The frontend is built with **Angular 20** and **PrimeNG** components, following modern reactive programming patterns with Angular Signals.

**Key Features:**
- **Framework**: Angular 20 with standalone components
- **UI Library**: PrimeNG for pre-built components
- **State Management**: Angular Signals for reactive state
- **Styling**: CSS with glassmorphism design system
- **Code Organization**: Code Flow Blocks pattern for maintainable, testable code
- **HTTP Client**: Angular HttpClient for API communication

**Directory Structure:**
- `src/app/` - Angular components, services, and modules
- `src/styles.css` - Global styles with design tokens
- `src/environments/` - Environment-specific configurations
- `public/` - Static assets

**Build & Serve:**
```bash
cd GoingMy.Web
npm install
npm start       # Development server at http://localhost:4200
npm run build   # Production build
npm run test    # Run unit tests
```

## Getting Started

### Prerequisites
- **.NET 10.0 SDK** or later (for backend services)
- **Node.js 22+** and **npm 11+** (for frontend)
- **PostgreSQL** (for database)
- **Redis** (for refresh token blacklist — provisioned automatically via Aspire)
- **Docker** (optional, for containerization)

### Quick Start

**Run the entire application (Web + Services):**

**Windows (PowerShell):**
```powershell
cd ..  # Go to workspace root
.\run.ps1        # Runs both services and web app
.\run.ps1 web    # Run only the web app
.\run.ps1 services  # Run only the .NET services
```

**macOS/Linux (Bash):**
```bash
cd ..  # Go to workspace root
chmod +x run.sh  # Make script executable (first time only)
./run.sh         # Runs both services and web app
./run.sh web     # Run only the web app
./run.sh services   # Run only the .NET services
```

**Access the Application:**
- **Web Application**: `http://localhost:4200`
- **API Gateway** (single entry point): `https://localhost:7000`
  - Proxies all REST API calls (`/api/*`) and WebSocket connections (`/hubs/*`)
  - Validates JWT tokens at the edge
  - Centralizes CORS and rate limiting
- **Auth Service API** (direct, for OIDC only): `https://localhost:7001`
  - Used directly by Angular for login (`/connect/authorize`, `/connect/token`)
- **User Service API** (direct): `https://localhost:7002`
- **Post Service API** (direct): `https://localhost:7003`
- **Chat Service API** (direct): `https://localhost:7004`
- **Aspire Dashboard**: Displayed in AppHost output

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
dotnet watch run
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
- JWT token generation and validation (OpenIddict PKCE)
- Identity fields only (`FirstName`, `LastName`, `IsActive`)
- Bootstraps a `UserProfile` in UserService after every signup
- Password management
- **Refresh token blacklist** via Redis (revoked tokens stored with TTL-based auto-expiry)
- **Admin management** — user list/search, activate/deactivate, user-level token revocation, registration stats (see `/api/admin/*`)
- Default admin account: `admin / admin123` (seeded on first run)

**Default API Port**: 5001

### User Service (`GoingMy.UserService`)
- Owns all non-authentication user data (single source of truth)
- User profile CRUD (`bio`, `avatarUrl`, `coverUrl`, `location`, `websiteUrl`, `gender`, `dateOfBirth`, `isPrivate`)
- Social counters (`followersCount`, `followingCount`, `postsCount`)
- Follow / Unfollow users
- Paginated followers & following lists
- Uses **PostgreSQL** via EF Core (relational follow graph)

**Default API Port**: 5002

### Post Service (`GoingMy.PostService`)
- Create, read, update, delete posts
- Post comments management
- Like/reaction handling
- Post feed management
- Media attachment support

**Default API Port**: 5003

### Chat Service (`GoingMy.ChatService`)
- Private one-to-one conversations
- Real-time messaging via SignalR hub (`/hubs/chat`)
- Message history retrieval
- Idempotent conversation creation (reuses existing conversations)
- MongoDB-backed message and conversation storage  - Consumes `UserUpdatedEvent` (sync username changes), `UserDeletedEvent` (remove from conversations)
**Default API Port**: 5004

### API Gateway (`GoingMy.ApiGateway`)
**Purpose**: Single public entry point for all backend services. Routes requests to downstream services, validates JWT tokens, enforces rate limiting, centralizes CORS, and forwards user context.

**Architecture**:
- **YARP Reverse Proxy**: Routes based on URL path patterns
- **OpenIddict JWT Validation**: Validates tokens against Auth Service issuer
- **Aspire Service Discovery**: Resolves downstream service addresses (`identity-api`, `user-api`, `post-api`, `chat-api`)
- **Rate Limiting**: 100 requests per 10 seconds per IP (HTTP 429)
- **CORS**: Centralized policy with `AllowCredentials` for SignalR
- **Claim Forwarding**: Extracts `sub` (user ID) and `name` (username) claims, forwards as `X-User-Id` and `X-Username` headers
- **WebSocket Proxying**: Enables SignalR hub communication through gateway

**Routes**:
| Path | Destination | Auth Required | Policy |
|------|-------------|---------------|--------|
| `/connect/*` | Auth Service | No | anonymous |
| `/api/user/*` | Auth Service | No | anonymous |
| `/api/userprofiles/*` | User Service | Yes | default |
| `/api/posts/*` | Post Service | Yes | default |
| `/api/chat/*` | Chat Service | Yes | default |
| `/hubs/*` | Chat Service (SignalR) | Yes | default |
| `/api/admin/*` | Auth Service | Yes | admin-policy |
| `/api/posts/admin/*` | Post Service | Yes | admin-policy |

**Default Gateway Port**: **7000** (HTTPS) / 5000 (HTTP)

**Important Notes**:
- OIDC login flows bypass the gateway—Angular's `angular-oauth2-oidc` calls Auth Service directly
- Downstream services retain independent JWT validation (defense in depth)
- Original `Authorization` header forwarded to all downstream services

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

### Backend Services

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

### Frontend (Angular Web)

```bash
# Navigate to web project
cd src/GoingMy.Web

# Install dependencies
npm install

# Start development server (http://localhost:4200)
npm start

# Build for production
npm run build

# Run unit tests
npm run test

# Run end-to-end tests
npm run e2e

# Generate new component
ng generate component components/MyComponent

# Generate new service
ng generate service services/my.service

# Format code with Prettier
npm run format

# Lint TypeScript files
npm run lint
```

### Full Stack Commands

```bash
# From workspace root - run everything
./run.sh          # macOS/Linux
.\run.ps1         # Windows

# Run just web
./run.sh web
.\run.ps1 web

# Run just services
./run.sh services
.\run.ps1 services
```

## Resources

### Backend
- **.NET Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/
- **Clean Architecture**: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **Microservices Patterns**: https://microservices.io/patterns/

### Frontend
- **Angular Documentation**: https://angular.io/docs
- **Angular Signals**: https://angular.io/guide/signals
- **PrimeNG Components**: https://primeng.org/
- **TypeScript Handbook**: https://www.typescriptlang.org/docs/
- **CSS Documentation**: https://developer.mozilla.org/en-US/docs/Web/CSS
- **Apple Human Interface Guidelines**: https://developer.apple.com/design/human-interface-guidelines/

## Support

For questions or issues:
1. Check the skill system documentation in `.instruction.md/`
2. Review the relevant service's code and tests
3. Consult the team lead or architecture team

---

**Created**: March 19, 2026  
**Last Updated**: May 5, 2026  
**Backend Technology**: .NET 10.0, PostgreSQL, MongoDB, Redis, RabbitMQ, xUnit, MediatR, OpenIddict, Scalar.AspNetCore, MassTransit, Aspire
**Frontend Technology**: Angular 20, PrimeNG, TypeScript, CSS  
**Architecture Pattern**: Clean Architecture with Microservices (Backend) + Signals & Code Flow Blocks (Frontend) + Event-Driven (RabbitMQ/MassTransit)
