namespace GoingMy.Shared.Events;

public record PostLikedEvent(
    string PostId,
    string PostAuthorUserId,
    string LikerUserId,
    string LikerUsername,
    string? LikerAvatarUrl);
