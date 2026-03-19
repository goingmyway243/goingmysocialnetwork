# SKILL: verify-api-contracts

## Purpose
Verify API contracts between microservices and client to ensure consistency, compatibility, and correctness of API designs.

## Use When
- Designing new APIs
- Making breaking changes
- Testing service integration
- Contract testing before deployment

## API Contract Definition

### Request/Response Format

**Standard Success Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com"
  },
  "meta": {
    "timestamp": "2024-03-19T10:00:00Z"
  }
}
```

**Standard Error Response:**
```json
{
  "success": false,
  "error": {
    "code": "USER_NOT_FOUND",
    "message": "User with ID 123 not found",
    "statusCode": 404
  },
  "meta": {
    "timestamp": "2024-03-19T10:00:00Z",
    "traceId": "0HN1GJ7IB4O4S:00000001"
  }
}
```

## HTTP Status Code Verification

| Status | Usage | Example |
|--------|-------|---------|
| 200 | GET successful | Retrieve user |
| 201 | POST created | User created |
| 204 | No content | Delete successful |
| 400 | Bad request | Invalid email format |
| 401 | Unauthorized | Missing auth token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not found | Unknown user ID |
| 409 | Conflict | Email already exists |
| 500 | Server error | Database error |

**Verification:**
```csharp
[Fact]
public async Task CreateUser_ReturnsStatusCreated() {
    var response = await client.PostAsJsonAsync(
        "/api/v1/users", 
        new { email = "test@example.com", name = "Test" }
    );
    Assert.Equal(201, (int)response.StatusCode);
}

[Fact]
public async Task GetInvalidUser_ReturnsNotFound() {
    var response = await client.GetAsync("/api/v1/users/99999");
    Assert.Equal(404, (int)response.StatusCode);
}
```

## API Versioning

**Url Path Versioning (Preferred):**
```
GET /api/v1/users          # User API v1
GET /api/v2/users          # User API v2 (breaking changes)
```

**Verification:**
- [ ] Version prefix in all endpoints
- [ ] v1 endpoints maintain backward compatibility
- [ ] v2 only deployed after v1 fully deprecated
- [ ] Versioning documented in API spec

## REST Naming Conventions

**Resource-Oriented Design:**
```
GET    /api/v1/users              # List users
POST   /api/v1/users              # Create user
GET    /api/v1/users/{id}         # Get specific user
PUT    /api/v1/users/{id}         # Update user
PATCH  /api/v1/users/{id}         # Partial update
DELETE /api/v1/users/{id}         # Delete user

GET    /api/v1/users/{id}/posts   # Get user's posts
GET    /api/v1/posts/{id}/comments # Get post comments
```

**Verification Checklist:**
- [ ] Endpoints are nouns, not verbs
- [ ] HTTP methods properly used
- [ ] Consistent naming across services
- [ ] URL hierarchies make sense
- [ ] Query parameters for filtering/pagination

## Query Parameters

**Pagination:**
```
GET /api/v1/users?page=1&limit=20
GET /api/v1/posts?skip=0&take=10
```

**Filtering:**
```
GET /api/v1/posts?userId=5&status=published
```

**Sorting:**
```
GET /api/v1/users?sort=createdAt:desc
GET /api/v1/posts?orderBy=createdAt&order=desc
```

## Request/Response Validation

**Valid Request:**
```csharp
public class CreateUserRequest {
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }
}
```

**Validation Test:**
```csharp
[Theory]
[InlineData("", "John")]           // Empty email
[InlineData("invalid@", "John")]   // Invalid email
[InlineData("test@test.com", "")]  // Empty name
public async Task CreateUser_InvalidData_ReturnsBadRequest(
    string email, string name) {
    var response = await client.PostAsJsonAsync(
        "/api/v1/users",
        new { email, name }
    );
    Assert.Equal(400, (int)response.StatusCode);
}
```

## Response Consistency

**DTO Structure:**
```csharp
public class UserDto {
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Consistency Verification:**
- [ ] Timestamp format consistent (ISO 8601)
- [ ] Null values handled consistently
- [ ] Date fields always present
- [ ] ID always included in responses
- [ ] Sensitive data never exposed (passwords, tokens)

## Contract Testing

**Example using Pact/Contract Pattern:**
```csharp
[Fact]
public async Task UserService_FollowsExpectedContract() {
    // Arrange
    var expectedContract = new {
        id = 1,
        email = "test@example.com",
        name = "Test User",
        createdAt = "2024-01-01T00:00:00Z"
    };
    
    // Act
    var response = await client.GetAsync("/api/v1/users/1");
    var user = await response.Content.ReadAsAsync<dynamic>();
    
    // Assert  
    Assert.NotNull(user.id);
    Assert.NotNull(user.email);
    Assert.IsType<DateTime>(
        DateTime.Parse(user.createdAt.ToString())
    );
}
```

## Error Handling Consistency

**Error Response Standard:**
```csharp
public class ErrorResponse {
    public bool Success { get; set; } = false;
    public Error Error { get; set; }
    public Meta Meta { get; set; }
}

public class Error {
    public string Code { get; set; }           // BUSINESS_RULE_VIOLATION
    public string Message { get; set; }        // User-friendly message
    public int StatusCode { get; set; }        // 400, 404, 500, etc
    public Dictionary<string, string> Details { get; set; }
}
```

**Verification:**
- [ ] All errors use same response structure
- [ ] Error codes machine-readable
- [ ] Messages user-friendly
- [ ] Status codes match HTTP standards
- [ ] Detailed error info in development, minimal in production

## Documentation: OpenAPI/Swagger

```csharp
/// <summary>
/// Get user by ID
/// </summary>
/// <param name="id">User ID</param>
/// <returns>User details</returns>
/// <response code="200">User found</response>
/// <response code="404">User not found</response>
[HttpGet("{id}")]
[ProduceResponseType(typeof(UserDto), 200)]
[ProduceResponseType(404)]
public async Task<IActionResult> GetUser(int id) { }
```

**Verification:**
- [ ] All endpoints documented
- [ ] Request/response examples provided
- [ ] Error codes explained
- [ ] Authentication requirements documented
- [ ] Rate limiting noted

## Quality Criteria
- All endpoints follow REST conventions
- Response format consistent across API
- Error handling standardized
- HTTP status codes correct
- Versioning strategy clear
- Documentation complete

## Verification Checklist
- [ ] OpenAPI/Swagger generates without errors
- [ ] Contract tests pass
- [ ] All status codes verified
- [ ] Error responses validated
- [ ] Pagination works for lists
- [ ] Filtering parameters work
- [ ] Authentication enforced
- [ ] Cross-service calls tested

## Edge Cases
- Listing empty collections (returns 200 with empty array)
- Concurrent create requests (handles duplicate detection)
- Missing optional fields (defaults provided)
- Timezone handling (all dates UTC)

## References
- REST API Design Rulebook
- OpenAPI Specification
- HTTP Status Code Reference

## Changelog
- v1.0: API contract verification framework
