using GoingMy.User.Domain.Enums;

namespace GoingMy.User.Application.Dtos;

public record UserProfileDto(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    string? Bio,
    string? AvatarUrl,
    string? CoverUrl,
    DateTime? DateOfBirth,
    Gender Gender,
    string? Location,
    string? WebsiteUrl,
    int FollowersCount,
    int FollowingCount,
    int PostsCount,
    bool IsVerified,
    bool IsPrivate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
