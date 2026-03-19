# SKILL: dotnet-coding-standards

## Purpose
Define coding standards, patterns, and best practices for .NET Core development in GoingMy to ensure consistency, maintainability, and quality.

## Use When
- Writing new service code
- Code review
- Establishing team coding guidelines
- Troubleshooting design issues

## Coding Conventions

### Project Structure
```
GoingMy.UserService/
├── src/
│   ├── GoingMy.User.API/          # ASP.NET Core API
│   ├── GoingMy.User.Domain/        # Business logic, entities
│   ├── GoingMy.User.Application/   # Use cases, DTOs
│   └── GoingMy.User.Infrastructure/ # DB, external services
├── tests/
│   ├── GoingMy.User.Tests/         # Unit tests
│   └── GoingMy.User.IntegrationTests/
└── GoingMy.UserService.sln
```

### Naming Conventions
- **Classes/Interfaces**: PascalCase
- **Methods**: PascalCase
- **Properties**: PascalCase
- **Private fields**: _camelCase
- **Local variables**: camelCase
- **Constants**: UPPER_SNAKE_CASE
- **Interfaces**: IPrefixPascalCase (e.g., IUserRepository)

### SOLID Principles Application

**S - Single Responsibility**
```csharp
// ❌ Bad: Multiple responsibilities
public class UserService {
    public void CreateUser(...) { }
    public void SendEmail(...) { }  // Email is separate concern
    public void UpdateDatabase(...) { }
}

// ✅ Good: Separated concerns
public class UserService { /* user logic */ }
public class EmailService { /* email logic */ }
public class UserRepository { /* data access */ }
```

**D - Dependency Injection**
```csharp
public class UserController {
    private readonly IUserService _userService;
    
    public UserController(IUserService userService) {
        _userService = userService;
    }
}
```

### Async/Await Best Practices
- Always use `async/await` for I/O operations
- Name async methods with `Async` suffix
- Use `ConfigureAwait(false)` in libraries

```csharp
public async Task<User> GetUserByIdAsync(int id) {
    return await _userRepository.GetByIdAsync(id).ConfigureAwait(false);
}
```

### Exception Handling
- Create custom exceptions for business logic
- Don't catch and ignore exceptions
- Log exceptions with context
- Return appropriate HTTP status codes

```csharp
public class UserNotFoundException : Exception {
    public UserNotFoundException(int userId) 
        : base($"User {userId} not found") { }
}
```

### CRUD Operations Pattern
```csharp
public interface IUserRepository {
    Task<User> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(int id);
}
```

## Validation & Sanitization
- Use FluentValidation for complex rules
- Validate inputs at API boundary
- Sanitize data to prevent injection attacks

## Configuration
- Use appsettings.json for configuration
- Environment-specific files: appsettings.{Environment}.json
- Use IConfiguration for dependency injection

## Logging
- Use structured logging (Serilog)
- Log at appropriate levels: Debug, Information, Warning, Error, Fatal
- Include correlation IDs for request tracing

## Testing Expectations
- Unit tests for business logic
- Integration tests for repositories
- Test coverage target: > 80%
- Use xUnit, Moq for testing

## Quality Criteria
- Code adheres to naming conventions
- SOLID principles respected
- No magic numbers or strings
- Comments explain WHY, not WHAT
- Error handling comprehensive
- Async operations used appropriately

## Verification Checklist
- [ ] All public methods have XML documentation
- [ ] No .Result or .Wait() blocking calls
- [ ] Exception handling covers edge cases
- [ ] Dependency injection properly configured
- [ ] Tests verify both happy and error paths

## Edge Cases
- Null reference handling
- Concurrent operation safety
- Database transaction boundaries
- API timeout scenarios

## References
- Microsoft Docs: C# Coding Conventions
- Clean Code by Robert C. Martin
- Async/Await Best Practices

## Changelog
- v1.0: Initial .NET coding standards
