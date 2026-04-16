# Changelog

All notable changes to the GoingMy Social Network project are documented in this file.

## [0.9.0] - 2026-04-16

### Added
- **Refresh token mechanism** across `GoingMy.AuthService` and `GoingMy.Web`:
  - **AuthService backend**:
    - `IRefreshTokenBlacklistService` / `RefreshTokenBlacklistService` — Redis-backed token revocation using `StackExchange.Redis`; stores revoked token JTIs with automatic TTL-based expiry (no cleanup job needed)
    - Refresh token revocation in `AuthorizationController.Logout()` — extracts JTI from authenticated principal and adds it to the Redis blacklist on every logout
    - Blacklist check in `HandleRefreshTokenGrantTypeAsync()` — rejects revoked tokens with `400 invalid_grant` before issuing new access tokens
    - `OpenIddict:IssueCookies` and `OpenIddict:RefreshTokenInCookie` configuration flags added to `appsettings.json`
  - **Angular frontend**:
    - `AuthService.initAuth()` now calls `OAuthService.setupAutomaticSilentRefresh()` — library-managed proactive refresh before token expiry (default 30-second margin)
    - `AuthService.refreshAccessToken()` — manual fallback wrapper over `OAuthService.refreshToken()` for edge cases
    - `refreshTokenInterceptor` (`refresh-token.interceptor.ts`) — functional HTTP interceptor; catches 401 responses and triggers logout as a safety net
    - Logout cancels the auto-refresh subscription before clearing tokens
    - Interceptor registered in `app.config.ts` after `authInterceptor` in the pipeline
- **Redis** added to `.NET Aspire` orchestration (`GoingMy.AppHost`):
  - `redis:7-alpine` container with persistent data volume (`goingmysocial-redis`)
  - `Aspire.Hosting.Redis` package added to `GoingMy.AppHost.csproj`
  - `StackExchange.Redis 2.7.27` added to `Directory.Packages.props` and `GoingMy.Auth.API.csproj`
  - `identity-api` configured with `.WithReference(redis)` and `.WaitFor(redis)`

### Changed
- **`AuthService` DI registrations** (`Program.cs`): replaced `RefreshTokenCleanupHostedService` hosted service with Redis `IConnectionMultiplexer` singleton; `IRefreshTokenBlacklistService` scoped registration retained
- **`ApplicationDbContext`**: removed `RefreshTokenBlacklist` DbSet and entity configuration (revocation data now lives in Redis, not PostgreSQL)
- **`AuthorizationController`**: constructor updated to inject `IRefreshTokenBlacklistService` and `ILogger<AuthorizationController>`
- **`angular-oauth2-oidc` usage**: replaced manual timer-based `scheduleTokenRefresh()` and BehaviorSubject-backed 401-retry logic with library's built-in `setupAutomaticSilentRefresh()`; significantly reduced frontend auth code complexity

### Architecture Notes
- **Redis TTL ≡ refresh token lifetime**: No background cleanup needed — revoked tokens auto-expire from Redis at the same time as the original token would have expired
- **Layered security**: Auth Service validates token against Redis blacklist on every refresh grant; 401 interceptor catches any access token expiry that slips through the proactive refresh window
- **HttpOnly cookie** is the intended storage for refresh tokens; browser sends it automatically on every `/connect/token` refresh request without JavaScript access

---

## [0.8.0] - 2026-04-15

### Added
- **Like feature** in `GoingMy.PostService`:
  - `Like` domain entity (`Entities/Like.cs`): Id, PostId, UserId, Username, CreatedAt
  - `ILikeRepository` domain interface with `ExistsAsync`, `GetByPostIdAsync`, `AddAsync`, `DeleteAsync`
  - `LikeRepository` MongoDB implementation with a **unique compound index `(PostId, UserId)`** enforcing one like per user per post at the database level
  - `LikePostCommand` — validates post exists, returns `409 Conflict` if already liked, atomically increments `Likes` counter via MongoDB `$inc`
  - `UnlikePostCommand` — validates like exists, atomically decrements `Likes` counter
  - `GetPostLikesQuery` — returns all likes for a post
  - `LikeDto` application DTO
  - REST endpoints on `PostsController`: `POST /api/posts/{id}/likes` → 201, `DELETE /api/posts/{id}/likes` → 204, `GET /api/posts/{id}/likes` → 200
- **Comment feature** in `GoingMy.PostService`:
  - `Comment` domain entity (`Entities/Comment.cs`): Id, PostId, UserId, Username, Content, CreatedAt, UpdatedAt; `Update(content)` method
  - `ICommentRepository` domain interface with full CRUD
  - `CommentRepository` MongoDB implementation with `(PostId, CreatedAt desc)` index for efficient paged reads
  - `AddCommentCommand` — validates post exists, atomically increments `Comments` counter
  - `UpdateCommentCommand` — ownership enforced (`UnauthorizedAccessException` → 403 if wrong user)
  - `DeleteCommentCommand` — ownership enforced, atomically decrements `Comments` counter
  - `GetCommentsByPostIdQuery` — returns comments sorted newest-first
  - `CommentDto` application DTO
  - New `CommentsController` at `[Route("api/posts/{postId}/comments")]`: `GET`, `POST` → 201, `PUT /{commentId}` → 200, `DELETE /{commentId}` → 204
- **Atomic post counters** on `IPostRepository` / `PostRepository`:
  - `IncrementLikesAsync`, `DecrementLikesAsync`, `IncrementCommentsAsync`, `DecrementCommentsAsync` — all use MongoDB `$inc` operator (race-condition safe; no load-then-save)
  - `DecrementLikesAsync` / `DecrementCommentsAsync` apply a `$gt: 0` filter guard to prevent negative counters
- **MongoDB collections** for `likes` and `comments` registered in `MongoDbContext` with indexes created in `InitializeAsync`

### Changed
- **`Author` record renamed to `User`** in `GoingMy.Post.Domain.Entities` (file [Author.cs](../src/GoingMy.PostService/src/GoingMy.Post.Domain/Entities/Author.cs) retains its filename; only the class name changed — no MongoDB data migration required as the document field name is determined by the C# *property* name `Author` on `Post`, which is unchanged)
- **`PostDto`** expanded with `int Likes`, `int Comments`, `UserDto? Author` fields; added `UserDto` record mirroring the `User` entity
- **PostDto construction centralised**: all four sites (`CreatePostCommand`, `UpdatePostCommand`, `GetPostsQuery`, `GetPostByIdQuery`) now delegate to a single `CreatePostCommandHandler.MapToDto()` helper

### Architecture Notes
- Likes and Comments are separate MongoDB collections (not embedded arrays in Post) to avoid unbounded document growth and enable efficient independent queries
- Post `likes`/`comments` integer fields are denormalized counters kept in sync via atomic `$inc` — they represent cached totals for feed display without requiring aggregation queries

---

## [0.7.0] - 2026-04-15

### Added
- **Event-driven user synchronization** across `PostService` and `ChatService` using **Apache Kafka** and **MassTransit**:
  - Shared event contracts in `GoingMy.Shared/Events/`:
    - `UserCreatedEvent` (UserId, Username, FirstName, LastName, AvatarUrl, IsVerified, CreatedAt)
    - `UserUpdatedEvent` (same fields + UpdatedAt)
    - `UserDeletedEvent` (UserId, Username, DeletedAt)
  - Kafka topic constants and consumer group constants added to `SharedServices` (`SharedServices.KafkaTopics`, `SharedServices.KafkaConsumerGroups`)
- **Outbox pattern** in `GoingMy.UserService` (exactly-once delivery guarantee):
  - `OutboxMessage` domain entity (`User.Domain/Outbox/OutboxMessage.cs`): Id, EventType, Payload (JSON), CreatedAt, PublishedAt, Error, RetryCount
  - `UserProfileOutboxInterceptor` EF Core `SaveChangesInterceptor` — automatically writes `OutboxMessage` rows within the same transaction as any `UserProfile` `Add`/`Modify`/`Delete` change; zero changes required to existing command handlers
  - `OutboxMessages` table added to `UserDbContext` with index on `PublishedAt` for efficient polling; EF Core migration `AddOutboxMessages` created and applied on startup
  - `UserDbContextDesignTimeFactory` added for EF Core tooling support (`dotnet ef migrations add`) without requiring the full Kafka DI graph
  - `OutboxPublisherWorker` hosted service (polls every 5 seconds, processes up to 50 messages per batch, max 5 retries, logs failures per entry)
- **Kafka producer** registered in `UserService` (`Program.cs`): `ITopicProducer<UserCreatedEvent>`, `ITopicProducer<UserUpdatedEvent>`, `ITopicProducer<UserDeletedEvent>` via MassTransit Kafka Rider
- **Kafka consumers** in `PostService`:
  - `UserCreatedEventConsumer` — log-only no-op (no pre-population needed)
  - `UserUpdatedEventConsumer` — calls `BulkUpdateAuthorAsync` to propagate username/avatar/verification changes to all posts via single MongoDB `UpdateMany`
  - `UserDeletedEventConsumer` — calls `MarkPostsAsDeletedUserAsync` to tombstone posts with `[deleted]` author placeholder (preserves post history)
  - Topic endpoints: `goingmy.user.created`, `goingmy.user.updated`, `goingmy.user.deleted` — consumer group `post-service`
- **Kafka consumers** in `ChatService`:
  - `UserUpdatedEventConsumer` — calls `BulkUpdateParticipantUsernameAsync` to sync username across all conversations the user participates in
  - `UserDeletedEventConsumer` — calls `RemoveParticipantAsync` to remove deleted user from participant lists (message history retained)
  - Consumer group `chat-service`
- **Bulk repository methods** (all use MongoDB driver directly, no load-then-save):
  - `PostService/IPostRepository`: `BulkUpdateAuthorAsync`, `MarkPostsAsDeletedUserAsync`
  - `ChatService/IConversationRepository`: `BulkUpdateParticipantUsernameAsync`, `RemoveParticipantAsync`
- **Kafka container** added to `GoingMy.AppHost`:
  - Bitnami Kafka image via `builder.AddKafka()` with Kafka UI sidecar and persistent data volume
  - `post-api`, `chat-api`, `user-api` each reference and `WaitFor(kafka)` before starting

### Changed
- **`Directory.Packages.props`**: Added `MassTransit 8.4.1`, `MassTransit.Kafka 8.4.1`, `MassTransit.EntityFrameworkCore 8.4.1`, `Aspire.Hosting.Kafka 13.1.1`
- **`GoingMy.AppHost.csproj`**: Added `Aspire.Hosting.Kafka` package reference
- **`UserService` `.csproj` files**: `GoingMy.User.Infrastructure` gains `MassTransit.EntityFrameworkCore` + `MassTransit.Kafka`; `GoingMy.User.API` gains `MassTransit.Kafka`
- **`PostService` / `ChatService` Application `.csproj` files**: Added `MassTransit.Kafka` and `GoingMy.Shared` project reference
- **`UserDbContext`**: Added `OutboxMessages` DbSet and `UserProfileOutboxInterceptor` interceptor registration; `Program.cs` updated to use `(sp, options)` lambda for interceptor injection
- **`PostService Program.cs`** / **`ChatService Program.cs`**: MassTransit Kafka Rider registered with consumer endpoints

### Architecture Notes
- **Zero command handler changes in UserService**: the `UserProfileOutboxInterceptor` automatically captures EF Core entity state changes — no explicit event publishing in business logic
- **Idempotent consumers**: MongoDB `UpdateMany` is safe to re-run; duplicate delivery does not cause data corruption
- **Tombstoning over deletion**: deleted users' posts display `[deleted]` author rather than being removed, preserving content history for community integrity

---

## [0.6.0] - 2026-04-08

### Added
- **YARP Reverse Proxy API Gateway** (`GoingMy.ApiGateway`): Centralized entry point for all backend services
  - New project: `src/GoingMy.ApiGateway/` with YARP 2.3.0 for reverse proxying
  - Aspire service discovery integration via `AddServiceDiscoveryDestinationResolver()`
  - **OpenIddict JWT validation at the gateway edge**: Validates tokens issued by Auth Service before forwarding to downstream services
  - **6 YARP routes** with Aspire-resolved cluster addresses:
    - `/connect/{**catch-all}` → `https://identity-api` (OIDC flows, anonymous)
    - `/api/user/{**catch-all}` → `https://identity-api` (Auth Service signup, anonymous)
    - `/api/userprofiles/{**catch-all}` → `https://user-api` (protected)
    - `/api/posts/{**catch-all}` → `https://post-api` (protected)
    - `/api/chat/{**catch-all}` → `https://chat-api` (protected)
    - `/hubs/{**catch-all}` → `https://chat-api` (SignalR WebSocket, protected)
  - **Centralized CORS policy**: Configured per `CorsOrigins` from `appsettings.json` with `AllowCredentials()` for SignalR upgrade
  - **Rate limiting**: Fixed-window limiter (100 requests per 10 seconds per IP, HTTP 429 rejection)
  - **Claim forwarding middleware**: Extracts authenticated user's `sub` and `name` claims, forwards as `X-User-Id` and `X-Username` headers to downstream services
  - **WebSocket support**: `app.UseWebSockets()` positioned before YARP to enable SignalR proxying
  - **Defense-in-depth**: Original `Authorization` header with JWT is forwarded to downstream services; they validate independently
  - HTTPS port: 7000, HTTP port: 5000
- **Chat Service re-enabled** in AppHost (`GoingMy.AppHost.cs`)
  - Uncommented ChatService orchestration
  - Fixed port conflict: changed from 5003/7003 (owned by Post Service) to **5004/7004**
  - Registered gateway with Aspire references to all four services
- **CORS removed from downstream services** (User, Post, Chat API `Program.cs`)
  - Centralized CORS enforcement at gateway
  - Auth Service retains CORS for direct OIDC flows (Angular oauth2-oidc library calls `/connect/token` directly)
- **Angular frontend updated** to use gateway URL:
  - `environment.ts`: Added `apiGatewayUrl: 'https://localhost:7000'` (kept `authConfig.issuer` pointing to Auth Service directly)
  - `environment.prod.ts`: Added `apiGatewayUrl: 'https://api.yourdomain.com'`
  - `user-api.service.ts`: Both `_authBaseUrl` and `_userBaseUrl` now read from `environment.apiGatewayUrl`
  - `dashboard-home.component.ts`: Posts endpoint URL now reads from `environment.apiGatewayUrl`

### Changed
- **AppHost.cs**: Restructured service registrations to capture `postService`, `userService`, `chatService` variables for gateway dependencies
- **Directory.Packages.props**: Added `Yarp.ReverseProxy 2.3.0` and `Microsoft.Extensions.ServiceDiscovery.Yarp 10.4.0`
- **GoingMy.Social.slnx**: Added `GoingMy.ApiGateway` project folder
- **GoingMy.AppHost.csproj**: Added ProjectReference to `GoingMy.ApiGateway`
- **Authentication flow**: Client calls to protected endpoints now route through gateway JWT validation; OIDC login still bypasses gateway

### Fixed
- **Port management**: Resolved conflict where Chat Service was attempting to use same ports (5003/7003) as Post Service

### Architecture Notes
- **Gateway responsibilities**: JWT validation, CORS, rate limiting, claim forwarding, WebSocket proxying
- **Downstream services**: Retain independent JWT validation (defense in depth), removed redundant CORS
- **OIDC flows**: Angular's `angular-oauth2-oidc` library continues to call Auth Service directly for `/connect/authorize`, `/connect/token`—the gateway does not interfere
- **Development access**: API calls to `https://localhost:7000` now centralized; individual service ports (5001-5004) still accessible directly if needed

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
