# Changelog

All notable changes to the GoingMy Social Network project are documented in this file.

## [0.3.0] - 2026-03-25

### Added
- **Scalar API integration**: Added Scalar.AspNetCore package to AuthService and PostService for interactive API documentation
- **CQRS pattern with MediatR**: Implemented complete Command Query Responsibility Segregation pattern in PostService
  - Consolidated command and handler classes in single files (`CreatePostCommand.cs`, `UpdatePostCommand.cs`, `DeletePostCommand.cs`)
  - Consolidated query and handler classes in single files (`GetPostsQuery.cs`, `GetPostByIdQuery.cs`)
  - Created `PostsController` with RESTful endpoints dispatching through MediatR
  - Applied exception handling patterns (UnauthorizedAccessException, InvalidOperationException) for proper HTTP status codes
- **Domain-driven design**: Created Post domain entity with business logic methods
- **Application DTOs**: Defined data transfer objects for API responses
- **Repository pattern**: Implemented `IPostRepository` interface with in-memory implementation (ready for EF Core migration)
- **Updated .instruction.md**: Comprehensive CQRS pattern documentation for AI agents including:
  - Command/Query/Handler consolidation guidelines
  - Controller pattern for REST endpoints
  - DTO separation from domain entities
  - Naming conventions and file organization
  - Exception handling patterns

### Changed
- **PostService architecture**: Refactored from minimal APIs to controller-based architecture
  - Replaced inline endpoint definitions with `PostsController` class
  - Improved code organization following clean architecture principles
  - Enhanced OpenAPI integration through controller attributes
- **Scalar version**: Updated to `2.13.14` in Directory.Packages.props for better API documentation
- **MediatR integration**: Added MediatR `12.4.1` package for CQRS pattern support
- **Program.cs**: Updated to register controllers, MediatR services, and repositories

### Fixed
- **Naming conflicts**: Separated request DTOs (CreatePostRequest, UpdatePostRequest) from domain commands to avoid ambiguity
- **Handler organization**: Removed separate Handlers directory, consolidating into command/query files for improved maintainability

## [0.2.0] - 2026-03-20

### Added
- **Pre-build verification checklist**: AI agents must validate all using statements before building projects
- **Documentation structure**: New `./docs/` folder for extended guidelines and architectural decisions
- **.instruction.md file**: Comprehensive AI agent instructions for working on GoingMy project
- **Code Flow Blocks pattern guidance**: Modern structured approach for Angular components
- **Glassmorphism design system reference**: Integration guidelines for Apple UI aesthetic in PrimeNG

### Changed
- **Project initialization**: Updated to use .NET 10 as the primary framework
- **Frontend architecture**: Shifted to Angular Signals for state management instead of traditional RxJS approaches
- **Aspire integration**: Emphasized for local development environment orchestration
- **Documentation approach**: Favor updating README.md over creating new files; use ./docs/ for extended content

### Fixed
- **Using statement validation process**: Added explicit checks before project builds to catch early errors

## [0.1.0] - 2026-03-19

### Added
- **Initial project scaffolding**: Created microservice architecture with Auth and Post services
- **Clean architecture structure**: Implemented Domain, Application, Infrastructure, and API layers
- **Service defaults configuration**: Shared configuration management across services
- **Test project structure**: Unit and integration test projects per service
- **AppHost setup**: .NET Aspire application host for local service orchestration
- **API documentation templates**: `.http` files for API testing and documentation

---

## Guidelines for VERSION Updates

- **MAJOR**: Breaking changes to API contracts or microservice architecture
- **MINOR**: New features, services, or substantial documentation updates  
- **PATCH**: Bug fixes, small improvements, or clarifications

## Contributing Changes

When adding entries to this changelog:
1. Use the existing format with Added, Changed, Fixed sections
2. Reference the specific service or component affected
3. Keep entries concise but descriptive
4. Update version number following semantic versioning
5. Add date of change in YYYY-MM-DD format
