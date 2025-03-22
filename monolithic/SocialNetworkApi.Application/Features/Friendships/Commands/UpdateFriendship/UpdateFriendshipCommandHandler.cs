using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Enums;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Friendships.Commands;

public class UpdateFriendshipCommandHandler : IRequestHandler<UpdateFriendshipCommand, CommandResultDto<FriendshipDto>>
{
    private readonly IRepository<FriendshipEntity> _friendshipRepository;
    private readonly IRepository<ChatroomEntity> _chatroomRepository;
    private readonly IMapper _mapper;

    public UpdateFriendshipCommandHandler(
        IRepository<FriendshipEntity> friendshipRepository,
        IRepository<ChatroomEntity> chatroomRepository,
        IMapper mapper
    )
    {
        _friendshipRepository = friendshipRepository;
        _chatroomRepository = chatroomRepository;
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
        if (request.Status == FriendshipStatus.Accepted)
        {
            await CreateChatroom(existingFriendship);
        }

        await _friendshipRepository.UpdateAsync(existingFriendship);

        var result = _mapper.Map<FriendshipDto>(existingFriendship);
        return CommandResultDto<FriendshipDto>.Success(result);
    }

    private async Task CreateChatroom(FriendshipEntity existingFriendship)
    {
        var existingChatroom = await _chatroomRepository.GetAll()
                        .FirstOrDefaultAsync(cr => cr.Participants.Any(p => p.UserId == existingFriendship.UserId)
                                && cr.Participants.Any(p => p.UserId == existingFriendship.FriendId));

        if (existingChatroom == null)
        {
            var chatroom = new ChatroomEntity()
            {
                Id = Guid.NewGuid(),
                Participants = new List<ChatroomParticipantEntity>()
                {
                    new ChatroomParticipantEntity { UserId = existingFriendship.UserId},
                    new ChatroomParticipantEntity { UserId = existingFriendship.FriendId}
                }
            };

            await _chatroomRepository.InsertAsync(chatroom);
        }
    }
}
