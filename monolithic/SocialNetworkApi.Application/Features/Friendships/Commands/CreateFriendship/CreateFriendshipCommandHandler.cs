using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Enums;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Friendships.Commands;

public class CreateFriendshipCommandHandler : IRequestHandler<CreateFriendshipCommand, CommandResultDto<FriendshipDto>>
{
    private readonly IRepository<FriendshipEntity> _friendshipRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public CreateFriendshipCommandHandler(
        IRepository<FriendshipEntity> friendshipRepository,
        IRepository<UserEntity> userRepository,
        IMapper mapper)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<FriendshipDto>> Handle(CreateFriendshipCommand request, CancellationToken cancellationToken)
    {
        var existingUsers = await _userRepository.GetAll()
            .Where(u => request.UserId == u.Id || request.FriendId == u.Id)
            .ToListAsync();

        if (existingUsers.Count != 2)
        {
            return CommandResultDto<FriendshipDto>.Failure("Invalid request, requested users cannot be found.");
        }

        var existingFriendship = await _friendshipRepository
            .FirstOrDefaultAsync(p =>
                (p.UserId == request.UserId && p.FriendId == request.FriendId)
                || (p.FriendId == request.UserId && p.UserId == request.FriendId)
            );

        if (existingFriendship != null)
        {
            return CommandResultDto<FriendshipDto>.Failure("You're already friends.");
        }

        var friendship = new FriendshipEntity() {
            Id= Guid.NewGuid(),
            UserId = request.UserId,
            FriendId = request.FriendId,
            Status = FriendshipStatus.Pending
        };

        await _friendshipRepository.InsertAsync(friendship);

        var result = _mapper.Map<FriendshipDto>(friendship);

        return CommandResultDto<FriendshipDto>.Success(result);
    }
}
