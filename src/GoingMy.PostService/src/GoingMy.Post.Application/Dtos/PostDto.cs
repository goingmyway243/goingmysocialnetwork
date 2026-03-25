namespace GoingMy.Post.Application.Dtos;

/// <summary>
/// Data Transfer Object for Post.
/// </summary>
public record PostDto(
    int Id,
    string Title,
    string Content,
    string UserId,
    string Username,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Request DTO for creating a new post.
/// </summary>
public record CreatePostRequestDto(
    string Title,
    string Content
);

/// <summary>
/// Request DTO for updating an existing post.
/// </summary>
public record UpdatePostRequestDto(
    string Title,
    string Content
);

/// <summary>
/// Response DTO for post operations.
/// </summary>
public record PostResponseDto(
    string Message,
    PostDto? Post = null
);
