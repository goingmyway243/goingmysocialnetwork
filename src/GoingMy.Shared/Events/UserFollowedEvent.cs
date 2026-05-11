namespace GoingMy.Shared.Events;

public record UserFollowedEvent(
    string FollowedUserId,
    string FollowedUsername,
    string FollowerUserId,
    string FollowerUsername);
