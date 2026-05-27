using GoingMy.Shared.Events;

namespace GoingMy.Search.API.Dtos;

public record UserSearchResultDto(
    string Id,
    string Username,
    string FirstName,
    string LastName,
    string? Bio,
    string? AvatarUrl,
    string? Location,
    int FollowersCount,
    bool IsVerified,
    bool IsPrivate);

public record PostSearchResultDto(
    string Id,
    string UserId,
    string Username,
    string Content,
    int Likes,
    int Comments,
    DateTime CreatedAt,
    IReadOnlyList<MediaAttachmentInfo>? MediaAttachments);
