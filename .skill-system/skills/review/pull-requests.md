# SKILL: review-pull-requests

## Purpose
Provide systematic approach for reviewing pull requests in GoingMy project to ensure code quality, consistency, and adherence to standards before merging.

## Use When
- Code review requested
- Pre-merge validation
- Establishing code quality standards
- Onboarding reviewers

## Review Strategy: Layers Approach

### Layer 1: Context & Scope (5 min)

**Questions:**
- [ ] PR title is descriptive?
- [ ] Description explains the "why", not just "what"?
- [ ] Scope is appropriate (not too small, not too large)?
- [ ] Related issues linked?
- [ ] No unrelated changes included?

**Example Good Description:**
```
## User Profile Avatar Upload Feature

### Problem
Users cannot personalize their profiles with avatars,
reducing engagement and platform attachment.

### Solution
- Add avatar upload endpoint (POST /api/v1/users/{id}/avatar)
- Implement S3 image storage with CDN
- Add avatar display in profile UI
- Validate image format and size

### Testing
- Unit tests for upload validation
- Integration tests for S3 storage
- E2E tests for UI upload flow

Fixes #1234
```

### Layer 2: Approach & Architecture (10 min)

**Questions:**
- [ ] Chosen approach follows architecture patterns?
- [ ] Solution aligns with existing code?
- [ ] Are there simpler alternatives?
- [ ] Database schema changes necessary?
- [ ] API contracts properly defined?
- [ ] Breaking changes documented?

**Example:**
```
// Reviewing avatar upload - checking approach

✅ Using existing IFileService abstraction (good reuse)
✅ S3 storage separate from User Service (good separation)
✅ Async processing for image optimization (good for perf)
❓ Why not using CDN directly? (question for clarification)
```

### Layer 3: Implementation Quality (15 min)

**Code Quality Checks:**

```csharp
// ❌ Issues to flag
public class AvatarController {
    public void UploadAvatar(IFormFile file) {
        // No async - blocks thread
        // No validation - security risk
        // No error handling
        var data = file.OpenReadStream().ReadAllBytes();
        S3.PutObject(data);
    }
}

// ✅ What to look for
public class AvatarController {
    [HttpPost("{userId}/avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatarAsync(
        int userId, 
        [FromForm] IFormFile file,
        CancellationToken ct) {
        
        try {
            // Input validation
            var validation = _avatarValidator.Validate(file);
            if (!validation.IsValid) {
                return BadRequest(validation.Errors);
            }
            
            // File storage
            var avatarUrl = await _fileService.UploadAsync(
                file, userId, ct
            );
            
            // Update user
            var user = await _userService.SetAvatarAsync(
                userId, avatarUrl, ct
            );
            
            return Ok(new AvatarResponse { Url = avatarUrl });
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Avatar upload failed");
            return StatusCode(500, "Upload failed");
        }
    }
}
```

**Points to Review:**
- [ ] Method names clear and descriptive
- [ ] Complexity reasonable (not too nested)
- [ ] Use of async/await correct
- [ ] Exception handling comprehensive
- [ ] Validation present
- [ ] Logging appropriate
- [ ] No TODO comments (should be issues)
- [ ] DRY principle followed
- [ ] Magic numbers extracted to constants
- [ ] Comments explain WHY, not WHAT

### Layer 4: Tests & Coverage (10 min)

**Test Review:**
```csharp
// ❌ Insufficient tests
[Fact]
public void TestUpload() {
    var controller = new AvatarController();
    var result = controller.Upload(null);
    Assert.True(result.Success);  // Too vague
}

// ✅ Comprehensive tests
[Theory]
[InlineData(null)]            // Null file
[InlineData("")]              // Empty file
[InlineData("invalid.txt")]   // Wrong format
public void UploadAvatar_InvalidFile_ReturnsBadRequest(
    string fileName) {
    // Arrange
    var mockFile = CreateMockFile(fileName);
    var controller = new AvatarController(mockValidator);
    
    // Act
    var result = controller.UploadAvatarAsync(1, mockFile);
    
    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
}

[Fact]
public async Task UploadAvatar_ValidFile_Succeeds() {
    // Arrange
    var mockFile = CreateMockFile("avatar.jpg");
    var mockS3 = new Mock<IS3Service>();
    mockS3.Setup(s => s.UploadAsync(...))
        .ReturnsAsync("https://cdn.example.com/avatar.jpg");
    
    var controller = new AvatarController(mockS3.Object);
    
    // Act
    var result = await controller.UploadAvatarAsync(1, mockFile);
    
    // Assert
    Assert.IsType<OkObjectResult>(result);
    mockS3.Verify(s => s.UploadAsync(...), Times.Once);
}
```

**Coverage Check:**
- [ ] Happy path tested
- [ ] Error paths tested
- [ ] Edge cases covered
- [ ] Coverage > 80%
- [ ] Test names clear
- [ ] No flaky tests

### Layer 5: Performance & Security (10 min)

**Performance Concerns:**
- [ ] N+1 queries? (check database calls)
- [ ] Large data transfers? (pagination, filtering)
- [ ] Memory leaks? (IDisposable properly used)
- [ ] Inefficient algorithms?

**Example Issue:**
```csharp
// ❌ N+1 Query Problem
foreach(var userId in userIds) {
    var posts = db.Posts
        .Where(p => p.UserId == userId)
        .ToList();  // Database call per iteration!
}

// ✅ Optimized
var posts = db.Posts
    .Where(p => userIds.Contains(p.UserId))
    .ToList();  // Single query
```

**Security Concerns:**
- [ ] Input validation present?
- [ ] SQL injection possible? (should use parameterized)
- [ ] XSS vulnerability? (output encoding)
- [ ] CSRF protection? (tokens for state-changing calls)
- [ ] Authorization checked? (not just authentication)
- [ ] Secrets in code? (API keys, passwords)
- [ ] Sensitive data logged? (PII, tokens)

### Layer 6: Documentation & Maintainability (5 min)

**Questions:**
- [ ] Public methods have XML documentation?
- [ ] Complex logic explained in comments?
- [ ] API documentation updated?
- [ ] README updated if needed?
- [ ] Database schema documented?

## Angular/Frontend Specific Checks

### Component Review
```typescript
// ❌ Issues
export class ProfileComponent {
  user: User;  // Not observable
  
  constructor(private service: UserService) {}
  
  ngOnInit() {
    this.service.getUser().subscribe(u => {
      this.user = u;  // Memory leak risk
    });
  }
}

// ✅ Good pattern
export class ProfileComponent implements OnInit, OnDestroy {
  user$ = new Observable<User>();
  private destroy$ = new Subject<void>();
  
  constructor(private service: UserService) {}
  
  ngOnInit() {
    this.user$ = this.service.getUser();
  }
  
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

### Template Review
```html
<!-- ❌ Problems -->
<div *ngIf="user">
  {{ user.password }}  <!-- Security breach -->
</div>

<!-- ✅ Good -->
<div *ngIf="user$ | async as user">
  {{ user.name }}  <!-- Safe, no password exposed -->
</div>
```

## Pull Request Review Checklist

```markdown
## Code Review Checklist

### Scope & Documentation
- [ ] PR description is clear
- [ ] Scope appropriate (not too large)
- [ ] Related issues linked
- [ ] No unrelated changes

### Architecture & Design
- [ ] Follows project patterns
- [ ] Database design appropriate
- [ ] API contracts correct
- [ ] No breaking changes or documented

### Implementation
- [ ] Code is readable and clear
- [ ] SOLID principles followed
- [ ] No code duplication
- [ ] Error handling present
- [ ] Logging adequate

### Testing
- [ ] Tests included
- [ ] Happy and error paths covered
- [ ] Edge cases tested
- [ ] Coverage adequate (>80%)

### Performance & Security
- [ ] No N+1 queries
- [ ] No memory leaks
- [ ] Input validation present
- [ ] No secrets in code
- [ ] SQL injection prevented
- [ ] Authorization checked

### Documentation
- [ ] Comments explain why
- [ ] XML docs on public APIs
- [ ] README updated if needed
- [ ] Complex logic explained

### Decision
- [ ] ✅ APPROVE
- [ ] ⚠️ REQUEST CHANGES
- [ ] ❌ REJECT
```

## Common Feedback Patterns

**Instead of:**
```
"This is wrong"
```

**Better:**
```
"Consider moving this validation to the service layer
following our pattern in UserService to improve reusability
and testability. Here's an example: [link]"
```

## Review Etiquette

- Be respectful and constructive
- Explain the "why" in feedback
- Acknowledge good solutions
- Ask questions rather than declaring
- Suggest improvements, don't demand
- Acknowledge learning opportunities

## Quality Criteria
- Review systematic and thorough
- Feedback constructive
- Issues clearly explained
- References provided
- Maintains code quality standards

## References
- Code Review Best Practices
- SOLID Principles
- Security Guidelines

## Changelog
- v1.0: Pull request review guidelines
