# SKILL: verify-code-quality-dotnet

## Purpose
Verify .NET code quality using standards from SOLID principles, code review patterns, and quality metrics to ensure maintainable, production-ready code.

## Use When
- Code review
- Pre-commit verification
- Merge request validation
- Establishing quality baselines

## Verification Framework: 4C Approach

### 1. Correctness - Is the code right?

**Logic Verification:**
- [ ] Business logic correctly implements requirements
- [ ] Algorithm is efficient (no N+1 queries, nested loops)
- [ ] Edge cases handled (null checks, empty collections)
- [ ] Exception handling appropriate
- [ ] Async operations don't block unnecessarily

**Code Pattern Checks:**
```csharp
// ❌ Bad: N+1 query problem
var users = context.Users.ToList();
foreach(var user in users) {
    var posts = context.Posts.Where(p => p.UserId == user.Id).ToList();
}

// ✅ Good: Include pattern
var users = context.Users.Include(u => u.Posts).ToList();
```

**SOLID Principles:**
- **Single Responsibility** - Class has one reason to change
- **Open/Closed** - Open for extension, closed for modification
- **Liskov Substitution** - Derived classes don't violate base contracts
- **Interface Segregation** - No fat interfaces
- **Dependency Inversion** - Depend on abstractions, not concretions

### 2. Completeness - Is nothing missing?

**Test Coverage:**
- [ ] Unit tests for public methods
- [ ] Integration tests for repositories
- [ ] Error path tests (try/catch scenarios)
- [ ] Coverage > 80% for new code

```csharp
[Fact]
public async Task CreateUser_WithNullEmail_ThrowsException() {
    // This test validates error handling
    var service = new UserService();
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => service.CreateAsync(null)
    );
}
```

**Documentation:**
- [ ] XML comments on public APIs
- [ ] Complex logic explained
- [ ] README with setup instructions

```csharp
/// <summary>
/// Creates a new user with validation
/// </summary>
/// <param name="email">User email address</param>
/// <returns>Created user entity</returns>
/// <exception cref="ArgumentException">If email invalid</exception>
public async Task<User> CreateUserAsync(string email) { }
```

**Configuration:**
- [ ] All settings in appsettings.json
- [ ] Environment-specific configs handled
- [ ] Secrets not committed

### 3. Context-Fit - Is it right for the domain?

**Domain Logic:**
- [ ] Code reflects business terminology
- [ ] Business rules correctly enforced
- [ ] Domain exceptions used appropriately

```csharp
// ❌ Bad: Generic exception
throw new Exception("User not found");

// ✅ Good: Domain exception
throw new UserNotFoundException(userId);
```

**Architecture Alignment:**
- [ ] Code follows layered architecture
- [ ] Service-to-service calls use interfaces
- [ ] Repositories abstract data access
- [ ] DTOs used for API boundaries

**API Contract:**
- [ ] Response format consistent
- [ ] Status codes correct (200, 201, 400, 404, 500)
- [ ] Error responses have proper structure

### 4. Consequence - Will it work in production?

**Performance:**
- [ ] No .Result or .Wait() blocking calls
- [ ] Queries have pagination for large result sets
- [ ] Unnecessary database calls eliminated
- [ ] Response times acceptable (< 200ms p95)

**Reliability:**
- [ ] Null reference guards
- [ ] Collection bounds checks
- [ ] Numeric overflow prevention
- [ ] String encoding handled

**Scalability:**
- [ ] Stateless service design
- [ ] No hard-coded values
- [ ] Connection pooling configured
- [ ] Caching strategy implemented

**Security:**
- [ ] Input validation present
- [ ] SQL injection prevented (parameterized queries)
- [ ] XSS prevention (output encoding)
- [ ] Authentication/authorization enforced

```csharp
// ❌ Bad: SQL injection vulnerable
var user = context.Users.FromSqlInterpolated(
    $"SELECT * FROM Users WHERE Email = '{email}'"
);

// ✅ Good: Parameterized query
var user = context.Users.FromSqlInterpolated(
    $"SELECT * FROM Users WHERE Email = {email}"
);
```

## Critical Questions Before Shipping

- [ ] **Can this code recover from failures?** (try-catch, retries)
- [ ] **Will this scale to 100K+ users?** (indexes, pagination)
- [ ] **Is sensitive data protected?** (encryption, hashing)
- [ ] **Are all dependencies documented?** (NuGet versions)
- [ ] **Does this create technical debt?** (workarounds, hacks)
- [ ] **Is this maintainable in 6 months?** (clear, documented)

## Automated Quality Tools

### Static Analysis
```bash
dotnet tool install -g dotnet-codeanalysis
dotnet codeanalysis run
```

### Unit Tests
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Code Formatting
```bash
dotnet format --verify-no-changes
```

## Quality Criteria
- All SOLID principles respected
- Test coverage > 80%
- No compiler warnings
- Code is self-documenting
- Error paths handled
- Performance acceptable

## Verification Checklist Template

```markdown
## Code Review Checklist

### Correctness (Logic)
- [ ] Logic matches requirements
- [ ] Edge cases handled
- [ ] No N+1 queries
- [ ] Exception handling adequate

### Completeness (Coverage)
- [ ] Tests pass
- [ ] Coverage > 80%
- [ ] Public APIs documented
- [ ] Configuration externalized

### Context-Fit (Domain)
- [ ] Business rules enforced
- [ ] Domain exceptions used
- [ ] API contracts correct
- [ ] Architecture respected

### Consequence (Production)
- [ ] No blocking calls
- [ ] Queries optimized
- [ ] Security validated
- [ ] Performance acceptable

### Decision
- [ ] ✅ APPROVED
- [ ] ⚠️ REQUEST CHANGES
- [ ] ❌ REJECT
```

## References
- Clean Code by Robert C. Martin
- SOLID Principles
- Microsoft Code Analysis Rules

## Changelog
- v1.0: .NET code quality verification framework
