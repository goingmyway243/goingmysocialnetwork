using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Friendships.Commands;

public class DeleteFriendshipCommandHandler : IRequestHandler<DeleteFriendshipCommand, CommandResultDto<Guid>>
{
    private readonly IRepository<FriendshipEntity> _friendshipRepository;

    public DeleteFriendshipCommandHandler(IRepository<FriendshipEntity> friendshipRepository)
    {
        _friendshipRepository = friendshipRepository;
    }

    public async Task<CommandResultDto<Guid>> Handle(DeleteFriendshipCommand request, CancellationToken cancellationToken)
    {
        var existingFriendship = await _friendshipRepository.GetByIdAsync(request.Id);
        if (existingFriendship == null)
        {
            return CommandResultDto<Guid>.Failure("Friendship not found.");
        }

        await _friendshipRepository.DeleteAsync(existingFriendship);
        return CommandResultDto<Guid>.Success(existingFriendship.Id);
    }
}
