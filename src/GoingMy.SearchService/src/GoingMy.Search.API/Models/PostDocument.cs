namespace GoingMy.Search.API.Models;

public record PostDocument(
     string Id,
     string Content,
     string UserId,
     string Username,
     int Likes,
     int Comments,
     DateTime CreatedAt,
     DateTime? UpdatedAt,
     bool UserHasLiked,
     IReadOnlyList<string>? MediaAttachments
);
