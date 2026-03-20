# Changelog

All notable changes to the GoingMy Social Network project are documented in this file.

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
