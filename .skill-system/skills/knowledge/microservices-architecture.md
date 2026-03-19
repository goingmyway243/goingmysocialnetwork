# SKILL: microservices-architecture-netcore

## Purpose
Establish foundational understanding of microservices architecture principles and how to design services for the GoingMy social network platform using .NET Core.

## Use When
- Planning a new service
- Reviewing service boundaries and responsibilities
- Evaluating service communication patterns
- Making architectural decisions

## Knowledge Area: Core Principles

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
- v1.0: Initial microservices architecture guidelines
