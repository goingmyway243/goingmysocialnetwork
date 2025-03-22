using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Friendships.Commands;

public class UpdateFriendshipCommandHandler : IRequestHandler<UpdateFriendshipCommand, CommandResultDto<FriendshipDto>>
{
    private readonly IRepository<FriendshipEntity> _friendshipRepository;
    private readonly IMapper _mapper;

    public UpdateFriendshipCommandHandler(
        IRepository<FriendshipEntity> friendshipRepository,
        IMapper mapper
    )
    {
        _friendshipRepository = friendshipRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<FriendshipDto>> Handle(UpdateFriendshipCommand request, CancellationToken cancellationToken)
    {
        var existingFriendship = await _friendshipRepository.GetByIdAsync(request.Id);
        if (existingFriendship == null)
        {
            return CommandResultDto<FriendshipDto>.Failure("Friendship not found.");
        }

        existingFriendship.Status = request.Status;

        await _friendshipRepository.UpdateAsync(existingFriendship);

        var result = _mapper.Map<FriendshipDto>(existingFriendship);
        return CommandResultDto<FriendshipDto>.Success(result);
    }
}
