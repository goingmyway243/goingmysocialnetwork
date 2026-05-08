using GoingMy.Shared.Events;
using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Repositories;
using MassTransit;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record UpdateAvatarCommand(Guid UserId, string AvatarUrl, string? OldAvatarFileId = null) : IRequest<UserProfileDto>;

public class UpdateAvatarCommandHandler(
    IUserProfileRepository userProfileRepository,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<UpdateAvatarCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User profile '{request.UserId}' not found.");

        profile.UpdateAvatar(request.AvatarUrl);

        var updated = await userProfileRepository.UpdateAsync(profile, cancellationToken);

        // Publish orphan event so UploadService can delete the old file
        if (!string.IsNullOrWhiteSpace(request.OldAvatarFileId))
        {
            await publishEndpoint.Publish(new FileOrphanedEvent
            {
                FileId = request.OldAvatarFileId
            }, cancellationToken);
        }

        return CreateUserProfileCommandHandler.MapToDto(updated);
    }
}
