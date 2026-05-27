using GoingMy.Shared.Events;

namespace GoingMy.Search.API.Dtos;

public record TrendingPostDto(
    string PostId,
    string UserId,
    string Username,
    string Content,
    int Likes,
    int Comments,
    int EngagementScore,
    DateTime CreatedAt,
    IReadOnlyList<MediaAttachmentInfo>? MediaAttachments);
