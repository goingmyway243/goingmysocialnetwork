# Changelog

All notable changes to the GoingMy Social Network project are documented in this file.

## [0.12.0] - 2026-04-29

### Added
- **User blocking feature** in `GoingMy.UserService`:
  - `UserBlock` domain entity (`User.Domain/Entities/UserBlock.cs`): composite key `(BlockerId, BlockeeId)`, `CreatedAt`, private parameterless constructor for EF Core
  - `IUserBlockRepository` interface: `ExistsAsync`, `CreateAsync`, `DeleteAsync`, `GetBlockedUserIdsAsync`
  - `UserBlockRepository` EF Core implementation using `context.UserBlocks`
  - `BlockUserCommand` / `UnblockUserCommand` CQRS commands (validate self-block, validate existence, delegate to repository)
  - `CheckBlockStatusQuery` returning `bool` for a given `(BlockerId, BlockeeId)` pair
  - EF Core migration `AddUserBlocks` — `UserBlocks` table with composite PK and indexes on both FK columns
  - `UserDbContext` updated with `DbSet<UserBlock> UserBlocks` and entity configuration
  - 4 new REST endpoints on `UserProfilesController`: `POST /{id}/block`, `DELETE /{id}/block`, `GET /{id}/is-blocked`, `GET /{id}/has-blocked-me`
  - `IUserBlockRepository` registered as scoped in `UserService Program.cs`
- **Follow & block validation** in `GoingMy.ChatService`:
  - `ChatController` now requires mutual follow relationship and rejects blocked users before allowing conversation creation (returns `403 Forbidden`)
  - Private helpers `IsFollowingAsync()` and `IsBlockedByAsync()` call `UserService` via named HTTP client with Bearer token forwarding
  - Named HTTP client `"user-api"` registered in `ChatService Program.cs` using Aspire service discovery address
- **Frontend blocking API** in `GoingMy.Web`:
  - `UserApiService`: added `blockUser()`, `unblockUser()`, `isBlocked()`, `hasBlockedMe()` methods
- **Direct messaging integration** in `GoingMy.Web`:
  - `ChatStateService`: added `NewMessageNotification` interface, `requestedConversationId` signal, `newMessageNotification` signal, and `openConversationWith()` method (joins SignalR conversation + loads messages + sets requested ID)
  - `MiniChatComponent`: two `effect()` calls using `untracked()` — one reacts to `requestedConversationId` to open the chatbox, one shows a PrimeNG Toast for background new-message notifications; `MessageService` provided locally
  - `ProfileHeaderComponent`: "Message" button shown when `isFollowing()` is true; emits `messageClick` output event
- **Dashboard layout control** via new `LayoutService` (`src/GoingMy.Web/src/app/services/layout.service.ts`):
  - `hideSidebar = signal(false)` — allows any child page to opt out of the sidebar
  - `DashboardComponent` subscribes to `NavigationStart` to reset `hideSidebar` to `false` on every navigation
  - Sidebar in `dashboard.component.html` wrapped in `@if (!layout.hideSidebar())` block

### Changed
- **AppHost.cs**: `userService` declaration moved before `chatService`; `.WithReference(userService)` added to `chatService` — fixes Aspire service discovery so `ChatService` can resolve `user-api` address
- **Angular routing** (`app.routes.ts`): `/profile/:userId` moved inside `dashboard` children as `profile/:userId` so the profile page renders within the dashboard shell (header + mini-chat widget); profile URL is now `/dashboard/profile/:userId`
- **Navigation calls updated** in 4 files (`profile.component.ts`, `discover.component.ts`, `dashboard-header.component.ts`, `dashboard-home.component.ts`): `/profile` → `/dashboard/profile`
- **ProfileComponent**: removed self-rendered `<app-dashboard-header>` (was causing duplicate header); injected `ChatStateService` and `LayoutService`; `ngOnInit` sets `hideSidebar.set(true)`; bound `(messageClick)="onMessageClick()"` to profile header

### Fixed
- **ChatService service discovery**: `user-api` address was never injected by Aspire because `chatService` was declared before `userService` in `AppHost.cs` and `.WithReference(userService)` was missing
- **Message button not opening mini-chat**: `openConversationWith()` was not calling `selectConversation()`, so SignalR was never joined and messages never loaded
- **Effect re-scheduling race**: `effect()` signal writes in `MiniChatComponent` wrapped with `untracked()` to prevent infinite re-scheduling
- **Mini-chat widget not in DOM on profile**: Profile was a top-level route outside the dashboard layout shell — moved inside dashboard children so `<app-mini-chat>` is always rendered
- **Duplicate header on profile page**: Profile component was rendering `<app-dashboard-header>` while the dashboard shell also rendered one

### Architecture Notes
- **Followers-only messaging**: Gateway enforces JWT; ChatService additionally validates mutual follow + no block before creating a conversation
- **LayoutService pattern**: Provides a clean signal-based mechanism for child pages to control dashboard-level layout features without tight coupling
- **`untracked()` in effects**: Required when an Angular `effect()` reads one signal and writes to another to prevent the effect from re-registering itself as a dependency on the write target

---

## [0.11.0] - 2026-04-28

### Added
- **Follow status check endpoint** in `GoingMy.UserService`:
  - New `CheckFollowStatusQuery` (Application/Queries/CheckFollowStatusQuery.cs): `record CheckFollowStatusQuery(Guid FollowerId, Guid FolloweeId) : IRequest<bool>` with handler using `IUserFollowRepository.ExistsAsync()`
  - New `CheckFollowStatus()` endpoint in `UserProfilesController`: `[HttpGet("{id:guid}/is-following")]` [Authorize] returns `bool`
  - Validates that caller is authenticated; extracts follower ID from JWT "sub" claim; returns true/false follow status

### Changed
- **UserProfileService.loadProfile()** (`GoingMy.Web` Angular service):
  - Enhanced to automatically check follow status when loading a profile
  - Uses `forkJoin()` to fetch profile + follow status in parallel
  - For authenticated users: both calls executed concurrently; `_isFollowing` signal updated with result
  - For anonymous users: profile-only fetch; `_isFollowing` defaults to false
  - Follow check errors gracefully handled with `catchError()` → defaults to false
  - Returns profile to maintain backward compatibility
- **UserApiService** Angular service: Added `checkIsFollowing(id: string): Observable<boolean>` method calling `/api/userprofiles/{id}/is-following`

### Architecture Notes
- **Zero cognitive load on consumers**: Profile loading automatically resolves follow status; UI components simply read the `isFollowing` computed signal
- **Parallel fetching**: forkJoin prevents additional round-trip latency—both HTTP requests sent concurrently
- **Graceful degradation**: Auth errors on follow-status check don't break profile load; defaults to false (user appears unfollowed if service fails)
- **Backward compatible**: loadProfile() still returns the profile Observable; follow status available via separate signal

---

## [0.10.0] - 2026-04-21

### Added
- **RabbitMQ event bus** replacing Apache Kafka for simplified event-driven architecture:
  - New `Aspire.Hosting.RabbitMQ` integration in `GoingMy.AppHost` for automatic container orchestration
  - `MassTransit.RabbitMQ` transport across all services (AuthService, UserService, PostService, ChatService)
  - Automatic queue creation by RabbitMQ broker (no pre-creation AdminClient code needed)

### Changed
- **Event bus migration from Kafka → RabbitMQ**:
  - All `MassTransit.Kafka` package references replaced with `MassTransit.RabbitMQ 8.4.1`
  - `Aspire.Hosting.Kafka` replaced with `Aspire.Hosting.RabbitMQ 13.1.1` in Directory.Packages.props
  - **AppHost.cs**: `builder.AddKafka()` → `builder.AddRabbitMQ("rabbitmq")` with automatic Aspire service discovery
  - **MassTransit configuration pattern**:
    - Removed Kafka rider pattern (AddRider, UsingKafka)
    - Simplified to direct `UsingRabbitMq()` with standard receive/publish endpoints
    - Connection string now sourced directly from Aspire: `cfg.Host(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!))`
    - No manual host/username/password configuration needed
  - **Publisher API change**: `ITopicProducer<T>` → `IPublishEndpoint` with `.Publish()` instead of `.Produce()`
    - Updated `AuthService/UserController.cs` (signup event publisher)
    - Updated `AuthService/UserSeeder.cs` (bootstrap admin with event)
    - Updated `UserService/OutboxPublisherWorker.cs` (outbox event dispatcher)
  - **Consumer endpoints**: All services now use `cfg.ReceiveEndpoint(queue_name, handler)` pattern instead of TopicEndpoint
    - UserService: `UserRegisteredEvent_consumer` queue
    - PostService: `UserCreatedEvent_consumer`, `UserUpdatedEvent_consumer`, `UserDeletedEvent_consumer` queues  
    - ChatService: `UserUpdatedEvent_consumer`, `UserDeletedEvent_consumer` queues
  - **Connection resolution**: Services now properly resolve RabbitMQ AMQP URI from Aspire's injected `ConnectionStrings__rabbitmq` configuration

### Architecture Notes
- **Simplified message broker**: RabbitMQ eliminates Kafka AdminClient topic pre-creation complexity; queues created on-demand
- **Aspire integration**: Automatic connection string injection with username/password handled by Aspire orchestration
- **No loss of semantics**: Event flow remains identical — signup → UserRegisteredEvent → UserService profile creation → UserCreatedEvent → Post/Chat sync
- **Production-ready**: Outbox pattern (UserService) ensures exactly-once event delivery; consumer idempotency unchanged

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
