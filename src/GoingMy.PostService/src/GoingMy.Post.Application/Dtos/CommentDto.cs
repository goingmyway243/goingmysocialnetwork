namespace GoingMy.Post.Application.Dtos;

public record CommentDto(
    string Id,
    string PostId,
    string UserId,
    string Username,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
