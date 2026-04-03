# Changelog

All notable changes to the GoingMy Social Network project are documented in this file.

## [0.5.0] - 2026-04-03

### Added
- **Liquid Glass Design System**: Complete Apple-inspired glassmorphism aesthetic for dashboard UI
  - 3-tier elevation system with CSS variables: `--glass-surface-bg` (0.08), `--glass-elevated-bg` (0.12), `--glass-overlay-bg` (0.18)
  - Performance-optimized blur scale: 8px/16px/24px with automatic performance mode for low-end devices
  - Comprehensive shadow elevation scale (sm/md/lg/xl) for depth hierarchy
  - Glass glow effects for hover/focus states
  - Utility classes: `.glass-surface`, `.glass-elevated`, `.glass-overlay`, `.glass-edge-top`, `.glass-glow`, `.glass-border-gradient`
  - Edge highlights and inner shadows for refined Apple-quality polish
- **Enhanced Background Gradients**: Three gradient options for dashboard container
  - Default: Deep purple-to-navy with enhanced warmth and contrast
  - `.bg-teal`: Midnight blue with teal accents
  - `.bg-light`: Slate to indigo (higher luminosity)
  - Animated radial overlay (0.15 opacity) and mesh gradient texture for added depth
- **Dashboard Header Redesign**: ([dashboard-header.component.css](../src/GoingMy.Web/src/app/components/dashboard-header/dashboard-header.component.css))
  - Applied `glass-elevated` styling with top edge highlight
  - Search input with glass effects and animated gradient border on focus
  - Create button with liquid gradient shimmer and glow effect
  - Icon buttons with refined hover states
  - Dropdown menu with `glass-overlay` styling
  - Fixed search icon positioning inside input field
- **Dashboard Sidebar Redesign**: ([dashboard-sidebar.component.css](../src/GoingMy.Web/src/app/components/dashboard-sidebar/dashboard-sidebar.component.css))
  - Applied `glass-surface` base with right edge gradient highlight
  - Navigation items with enhanced active state (0.20 opacity + gradient border)
  - Glass orb backgrounds for icons with pulsing glow animation
  - Create Post button with liquid gradient shimmer animation
  - Improved icon styling with visible glass spheres and backdrop effects
- **Post Cards Multi-Layer Glass Effects**: ([dashboard-home.component.css](../src/GoingMy.Web/src/app/pages/dashboard/dashboard-home/dashboard-home.component.css))
  - Base glass-elevated with top and bottom edge effects
  - Gradient border on hover with enhanced lift animation (-4px translateY)
  - Refined author avatar with gradient ring and inner highlight
  - Glass action buttons with color-coded hover states (blue/red)
  - Loading shimmer animation and error/empty states with glass effects
- **Micro-Interactions System**: Smooth, performant animations across all components
  - `glassShimmer`: Loading effect with moving gradient
  - `glowPulse`: Pulsing glow for active states
  - Standardized transitions: `--transition-smooth` (cubic-bezier), `--transition-fast`
  - Keyboard focus rings with gradient styling
  - Full `prefers-reduced-motion` support
- **PrimeNG Theme Integration**: ([app.theme.ts](../src/GoingMy.Web/src/app/configs/app.theme.ts))
  - Extended Lara theme with glassmorphism tokens
  - Mapped surface tokens to CSS variables for consistency
  - Global styling properties for border radius and transitions
- **Accessibility & Performance**:
  - WCAG AA contrast ratios maintained on all text
  - GPU acceleration with `will-change` hints on interactive elements
  - Performance mode (`.glass-performance-mode`) reduces blur for low-end devices
  - Browser compatibility: Chrome/Edge, Safari (-webkit-), Firefox

### Changed
- **Global Styles**: Replaced old glass class with modern 3-tier elevation CSS variable system
  - Updated all form inputs with enhanced glass effects
  - Improved button styling with glow and lift animations
  - Enhanced checkbox styling with backdrop filters
- **CSS Variables**: Comprehensive redesign of design tokens
  - Added 40+ new CSS variables for glass effects, colors, shadows, and transitions
  - Maintained backward compatibility with legacy variable names

### Fixed
- **Header CSS**: Cleaned up corrupted duplicate code sections in dashboard-header.component.css
- **Search Icon Positioning**: Fixed icon positioning inside search input field
  - Icon now properly positioned with flexbox container
  - Added `pointer-events: none` to prevent blocking input interactions

## [0.4.0] - 2026-03-31

### Added
- **ChatService**: New microservice for real-time chat and messaging
  - Clean architecture scaffold: `GoingMy.Chat.Domain`, `GoingMy.Chat.Application`, `GoingMy.Chat.Infrastructure`, `GoingMy.Chat.API`
  - `Conversation` and `Message` domain entities with business logic methods
  - `IConversationRepository` and `IMessageRepository` domain interfaces
  - CQRS commands: `SendMessageCommand`, `CreateConversationCommand`
  - CQRS queries: `GetConversationsQuery`, `GetConversationMessagesQuery`
  - `ConversationDto` and `MessageDto` application DTOs
  - MongoDB `MongoDbContext` with compound indexes on conversations and messages
  - `ConversationRepository` and `MessageRepository` MongoDB implementations
  - `ChatController` REST API: `GET /api/chat/conversations`, `POST /api/chat/conversations`, `GET /api/chat/conversations/{id}/messages`, `POST /api/chat/conversations/{id}/messages`
  - `ChatHub` SignalR hub at `/hubs/chat` for real-time message broadcasting
  - Idempotent conversation creation (returns existing conversation between two participants)
  - OpenIddict JWT validation (same issuer as PostService)
  - `.http` file for manual API testing
  - Unit and integration test project stubs
- **SharedServices constants**: Added `ChatApi = "chat-api"` and `ChatDb = "chat-db"`
- **AppHost orchestration**: ChatService registered with MongoDB `chat-db` database and identity issuer env variable
- **Solution files**: `GoingMy.ChatService.slnx` and all four projects added to `GoingMy.Social.slnx`

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
- **MINOR**: New features, services, substantial UI redesigns, or documentation updates  
- **PATCH**: Bug fixes, small improvements, or clarifications

## Contributing Changes

When adding entries to this changelog:
1. Use the existing format with Added, Changed, Fixed sections
2. Reference the specific service or component affected
3. Keep entries concise but descriptive
4. Update version number following semantic versioning
5. Add date of change in YYYY-MM-DD format
