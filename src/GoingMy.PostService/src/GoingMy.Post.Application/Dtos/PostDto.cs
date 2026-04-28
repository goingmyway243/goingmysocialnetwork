namespace GoingMy.Post.Application.Dtos;

/// <summary>
/// Denormalized user info embedded in post responses.
/// </summary>
public record UserDto(
    string Id,
    string UserName,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    bool IsVerified
);

/// <summary>
/// Data Transfer Object for Post.
/// </summary>
public record PostDto(
    string Id,
    string Content,
    string UserId,
    string Username,
    int Likes,
    int Comments,
    UserDto? Author,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool UserHasLiked = false
);

/// <summary>
/// Request DTO for creating a new post.
/// </summary>
public record CreatePostRequestDto(
    string Content
);

/// <summary>
/// Request DTO for updating an existing post.
/// </summary>
public record UpdatePostRequestDto(
    string Content
);

/// <summary>
/// Response DTO for post operations.
/// </summary>
public record PostResponseDto(
    string Message,
    PostDto? Post = null
);
