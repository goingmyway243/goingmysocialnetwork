using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Commands;

public record UpdateCoverCommand(Guid UserId, string CoverUrl) : IRequest<UserProfileDto>;

public class UpdateCoverCommandHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<UpdateCoverCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateCoverCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User profile '{request.UserId}' not found.");

        profile.UpdateCover(request.CoverUrl);

        var updated = await userProfileRepository.UpdateAsync(profile, cancellationToken);

        return CreateUserProfileCommandHandler.MapToDto(updated);
    }
}
