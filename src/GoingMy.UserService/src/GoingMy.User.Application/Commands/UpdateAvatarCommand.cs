using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record UpdateAvatarCommand(Guid UserId, string AvatarUrl) : IRequest<UserProfileDto>;

public class UpdateAvatarCommandHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<UpdateAvatarCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User profile '{request.UserId}' not found.");

        profile.UpdateAvatar(request.AvatarUrl);

        var updated = await userProfileRepository.UpdateAsync(profile, cancellationToken);

        return CreateUserProfileCommandHandler.MapToDto(updated);
    }
}
