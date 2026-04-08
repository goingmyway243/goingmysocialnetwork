using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Enums;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record UpdateUserProfileCommand(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Bio,
    DateTime? DateOfBirth,
    Gender? Gender,
    string? Location,
    string? WebsiteUrl,
    bool? IsPrivate
) : IRequest<UserProfileDto>;

public class UpdateUserProfileCommandHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<UpdateUserProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User profile with ID '{request.Id}' was not found.");

        profile.UpdateProfile(
            firstName: request.FirstName,
            lastName: request.LastName,
            bio: request.Bio,
            dateOfBirth: request.DateOfBirth,
            gender: request.Gender,
            location: request.Location,
            websiteUrl: request.WebsiteUrl,
            isPrivate: request.IsPrivate);

        var updated = await userProfileRepository.UpdateAsync(profile, cancellationToken);

        return CreateUserProfileCommandHandler.MapToDto(updated);
    }
}
