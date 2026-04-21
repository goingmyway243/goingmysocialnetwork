using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Entities;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Commands;

/// <summary>
/// Creates a new user profile. Called by AuthService immediately after a successful signup.
/// </summary>
public record CreateUserProfileCommand(
    Guid Id,
    string Username,
    string FirstName,
    string LastName
) : IRequest<UserProfileDto>;

public class CreateUserProfileCommandHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<CreateUserProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // Idempotent: if the profile already exists, return it as-is (handles duplicate RabbitMQ delivery).
        var existing = await userProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing != null)
            return MapToDto(existing);

        var profile = new UserProfile(
            id: request.Id,
            username: request.Username,
            firstName: request.FirstName,
            lastName: request.LastName
        );

        var created = await userProfileRepository.CreateAsync(profile, cancellationToken);

        return MapToDto(created);
    }

    internal static UserProfileDto MapToDto(UserProfile p) => new(
        Id: p.Id,
        Username: p.Username,
        FirstName: p.FirstName,
        LastName: p.LastName,
        Bio: p.Bio,
        AvatarUrl: p.AvatarUrl,
        CoverUrl: p.CoverUrl,
        DateOfBirth: p.DateOfBirth,
        Gender: p.Gender,
        Location: p.Location,
        WebsiteUrl: p.WebsiteUrl,
        FollowersCount: p.FollowersCount,
        FollowingCount: p.FollowingCount,
        PostsCount: p.PostsCount,
        IsVerified: p.IsVerified,
        IsPrivate: p.IsPrivate,
        IsActive: p.IsActive,
        CreatedAt: p.CreatedAt,
        UpdatedAt: p.UpdatedAt);
}
