# SKILL: microservices-architecture-netcore

## Purpose
Establish foundational understanding of microservices architecture principles and how to design services for the GoingMy social network platform using .NET Core with .NET Aspire orchestration.

## Use When
- Planning a new service
- Reviewing service boundaries and responsibilities
- Evaluating service communication patterns
- Making architectural decisions
- Setting up service observability and monitoring

## Knowledge Area: Core Principles

### .NET Aspire for Local Development & Orchestration

GoingMy uses **.NET Aspire** to orchestrate local development and cloud-native deployments:

**Key Components:**
- **AppHost Project** - Centralized orchestration of all services, databases, and caches
- **Aspire Dashboard** - Real-time monitoring (health, logs, traces, metrics)
- **Service Defaults** - Shared configuration for observability (OpenTelemetry)
- **Auto Service Discovery** - Services automatically discover each other's endpoints

**Benefits:**
- Single command to start entire environment: `cd AppHost && dotnet run`
- No manual Docker-Compose management
- Consistent local and cloud deployment patterns
- Built-in observability (traces, logs, metrics)
- Automatic health monitoring

**Development Flow:**
```
AppHost (runs all services)
├── PostgreSQL (via Aspire.Hosting.PostgreSQL)
├── Redis (via Aspire.Hosting.Redis)
├── User Service → Dashboard port: 5001
├── Post Service → Dashboard port: 5002
├── Notification Service → Dashboard port: 5003
└── ... other services

Monitor via Dashboard: http://localhost:18888
```

### Service Registration in Aspire AppHost

Each microservice is registered in the central AppHost for orchestration:

```csharp
// AppHost/Program.cs - Central service registration
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres");
var userDb = postgres.AddDatabase("goingmy-user-db");
var postDb = postgres.AddDatabase("goingmy-post-db");
var redis = builder.AddRedis("redis");

// Services
var userService = builder
    .AddProject<Projects.GoingMy_User_API>("user-service")
    .WithReference(userDb)
    .WithReference(redis)
    .WithHttpEndpoint(port: 5001);

var postService = builder
    .AddProject<Projects.GoingMy_Post_API>("post-service")
    .WithReference(postDb)
    .WithReference(redis)
    .WithHttpEndpoint(port: 5002)
    .WithReference(userService);  // Service-to-service dependency

await builder.Build().RunAsync();
```

### Service Boundaries
Each microservice should:
- Handle a single business capability (User service, Post service, etc.)
- Have its own database (no shared databases between services)
- Be independently deployable and scalable
- Have clear API contracts
- Be loosely coupled with other services

### Service Communication
**Synchronous (HTTP/REST):**
- API calls between services
- Use for immediate responses
- Add retry logic and circuit breaker pattern
- Implement request/response timeouts

**Asynchronous (Message Queue):**
- Event-driven architecture
- Use RabbitMQ or Azure Service Bus
- Example: Post creation → trigger notifications

### API Design Standards
- RESTful endpoints: `/api/v1/{resource}`
- Standard HTTP methods: GET, POST, PUT, DELETE, PATCH
- Versioning strategy: URL path version prefix
- Standard response format with metadata
- Proper HTTP status codes (200, 201, 400, 404, 500)

### Data Management Pattern
```
┌──────────────────┐
│   API Gateway    │
└────────┬─────────┘
         │
    ┌────┴────────────────────┐
    ▼                          ▼
┌─────────────┐        ┌──────────────┐
│ User Service│        │ Post Service │
│ User DB     │        │ Post DB      │
└─────────────┘        └──────────────┘
```

## GoingMy Services Structure
- **API Gateway** - Route requests, handle auth
- **User Service** - Authentication, profiles, relationships
- **Post Service** - Create, read, update, delete posts
- **Notification Service** - User notifications
- **Message Service** - Direct messaging
- **Feed Service** - Personalized feeds (read model)
- **Search Service** - Content search

## Quality Criteria
- Services have single responsibility
- Clear domain boundaries documented
- API contracts versioned and documented
- Inter-service calls include error handling
- Data consistency strategy defined

## References
- Sam Newman: Building Microservices, 2nd Edition
- Microsoft: .NET Microservices Architecture
- API Design Best Practices

## Changelog
- v2.0: Added .NET Aspire orchestration, AppHost service registration, OpenTelemetry observability
- v1.0: Initial microservices architecture guidelines
