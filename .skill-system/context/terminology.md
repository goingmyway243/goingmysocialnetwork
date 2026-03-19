# Project Terminology

## Architectural Terms
- **Microservice** - Independent, deployable service with single responsibility
- **API Gateway** - Entry point routing requests to appropriate services
- **Service-to-Service Communication** - Inter-service HTTP/gRPC calls
- **Circuit Breaker** - Pattern preventing cascading failures

## Domain Terms (Social Network)
- **User** - Individual platform member with profile
- **Post** - Content piece (text, image, link) shared by user
- **Feed** - Personalized content stream for a user
- **Engagement** - User action on content (like, comment, share)
- **Notification** - Event alert to user (mention, follow, like)
- **Connection** - Follow relationship between users

## UI/Design Terms
- **Glassmorphism** - Translucent, frosted glass aesthetic design trend
- **Component** - Reusable UI building block
- **Design Token** - Design decision captured as variable (color, spacing, typography)
- **Accessibility** - WCAG 2.1 AA compliance

## Development Terms
- **SOLID** - Design principles for clean code
- **DDD** - Domain-Driven Design for complex business logic
- **CQRS** - Command Query Responsibility Segregation pattern
- **Repository** - Data access abstraction layer
- **DTO** - Data Transfer Object for API contracts

## Angular Modern Terms
- **Signal** - Reactive primitive for local component state (preferred over Observable for state)
- **Computed Signal** - Derived signal that auto-updates based on dependencies
- **Effect** - Runs code when signals change
- **input()** - Modern function to define component inputs (replaces @Input decorator)
- **output()** - Modern function to define component outputs (replaces @Output decorator)
- **Control Flow Block** - Template syntax like @if, @for, @switch (modern alternative to *ngIf, *ngFor)
- **Standalone Component** - Component that doesn't require a module (modern approach)

## Testing Terms
- **Unit Test** - Test single function/method in isolation
- **Integration Test** - Test multiple components working together
- **Contract Test** - Test API contracts between services
- **E2E Test** - End-to-end user flow testing
