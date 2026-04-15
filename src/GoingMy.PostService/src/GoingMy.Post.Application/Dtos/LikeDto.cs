namespace GoingMy.Post.Application.Dtos;

public record LikeDto(
    string Id,
    string PostId,
    string UserId,
    string Username,
    DateTime CreatedAt
);
