---
applyTo: "src/**/*.{cs,csproj,slnx,http}"
---

# Backend Development Rules — GoingMy Social Network

> **Agent**: Always load and follow these rules when working on any `.cs`, `.csproj`, `.slnx`, or `.http` file in the `src/` directory.
> Use **Context7 MCP** to fetch up-to-date documentation for .NET 10, MediatR, and Aspire before generating code.

---

## Tech Stack
- **Framework**: .NET 10
- **Architecture**: Microservices
- **Orchestration**: .NET Aspire (local dev)
- **Patterns**: CQRS via MediatR, Repository pattern, Dependency Injection

---

## Service Locations

| Service | Responsibility | Location |
|---------|----------------|----------|
| AuthService | User authentication & authorization | `src/GoingMy.AuthService/` |
| PostService | Social content & interactions | `src/GoingMy.PostService/` |
| ServiceDefaults | Shared configurations & extensions | `src/GoingMy.ServiceDefaults/` |
| AppHost | Aspire orchestration host | `src/GoingMy.AppHost/` |

---

## Project Structure (Per Service)

```
ServiceName/
├── src/
│   ├── ServiceName.API/            # Controllers, HTTP endpoints
│   ├── ServiceName.Application/    # Commands, Queries, DTOs
│   ├── ServiceName.Domain/         # Entities, value objects
│   └── ServiceName.Infrastructure/ # Data access, external integrations
└── tests/
    ├── ServiceName.Tests/           # Unit tests
    └── ServiceName.IntegrationTests/
```

---

## CQRS Pattern (MediatR)

### Commands — Write Operations
- Location: `Application/Commands/[ActionName]Command.cs`
- Each file contains **both** the command record and its handler class
- Record: implements `IRequest<TResponse>`
- Handler: named `[CommandName]Handler`, implements `IRequestHandler<TCommand, TResponse>`
- Validate ownership → throw `UnauthorizedAccessException`
- Resource not found → throw `InvalidOperationException`

```csharp
// Example: CreatePostCommand.cs
public record CreatePostCommand(string UserId, string Content) : IRequest<PostDto>;

public class CreatePostCommandHandler(IPostRepository repository)
    : IRequestHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostCommand command, CancellationToken ct)
    {
        // business logic here
    }
}
```

### Queries — Read Operations
- Location: `Application/Queries/Get[Resource][Criteria]Query.cs`
- Each file contains **both** the query record and its handler class
- Handler named `[QueryName]Handler`
- Return `null` or empty collections for missing resources

```csharp
// Example: GetPostByIdQuery.cs
public record GetPostByIdQuery(Guid PostId) : IRequest<PostDto?>;

public class GetPostByIdQueryHandler(IPostRepository repository)
    : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    public async Task<PostDto?> Handle(GetPostByIdQuery query, CancellationToken ct)
    {
        // retrieval logic here
    }
}
```

### Application Layer Layout
```
ServiceName.Application/
├── Commands/
│   ├── CreatePostCommand.cs
│   ├── UpdatePostCommand.cs
│   └── DeletePostCommand.cs
├── Queries/
│   ├── GetPostsQuery.cs
│   └── GetPostByIdQuery.cs
└── Dtos/
    └── PostDto.cs
```

### Adding a New Command or Query
1. Create file in `Commands/` or `Queries/` following naming conventions
2. Define the record with `IRequest<TResponse>`
3. Define the handler with `IRequestHandler<T, TResponse>`
4. Inject `IPostRepository` or other dependencies via constructor
5. Implement `Handle()` method
6. Add a controller action that calls `mediator.Send(new YourCommand(...))`
7. MediatR auto-discovers handlers via assembly scanning in `Program.cs`

---

## Controllers

- Single responsibility: HTTP request → claim extraction → MediatR dispatch → HTTP response
- Use **separate request DTOs** from domain commands for API contracts
- Annotate with `[ProducesResponseType]` for OpenAPI documentation
- Map specific exceptions to HTTP status codes

```csharp
[HttpPost]
[ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    try
    {
        var result = await mediator.Send(new CreatePostCommand(userId, request.Content));
        return CreatedAtAction(nameof(GetPost), new { id = result.Id }, result);
    }
    catch (UnauthorizedAccessException) { return Unauthorized(); }
}
```

---

## DTOs

- Located in `Application/Dtos/`
- Use `record` types for immutability
- Keep minimal — only what the API needs to return
- Never expose domain entities directly

---

## Code Standards

- Follow Microsoft .NET naming conventions
- Enable nullable reference types: `#nullable enable`
- Constructor injection for all dependencies
- `async`/`await` for all I/O operations
- Write unit and integration tests for business logic
- Remove unused `using` statements

---

## Code Conventions
- Use `var` when the type is obvious from the right-hand side
- Use expression-bodied members for simple methods
- Use pattern matching and switch expressions where appropriate
- Keep methods short and focused (max ~20 lines)
- Use pascal case for public members, camel case for private fields (with underscore prefix)

---

## Pre-Build Checklist

Before building or submitting any backend change:
- [ ] All `using` statements in modified files are correct and necessary
- [ ] All referenced NuGet packages exist in `.csproj` and versions are in `Directory.Packages.props`
- [ ] Run `dotnet build` to catch compile errors early
- [ ] Run `dotnet test` in the service directory

---

## Development Experience

- Use .NET Aspire (`GoingMy.AppHost`) to run all services locally
- Check `appsettings.Development.json` for local config overrides
- All services expose health checks and structured logging
- API contracts are documented in `.http` files (e.g., `GoingMy.Auth.API.http`)

---

## Context7 MCP — Backend Libraries

When working on backend code, **always** fetch current docs for:

| Library | How to resolve |
|---------|----------------|
| .NET 10 / ASP.NET Core | `resolve-library-id: "aspnetcore"` |
| MediatR | `resolve-library-id: "MediatR"` |
| .NET Aspire | `resolve-library-id: "dotnet aspire"` |
| Entity Framework Core | `resolve-library-id: "entity framework core"` |
