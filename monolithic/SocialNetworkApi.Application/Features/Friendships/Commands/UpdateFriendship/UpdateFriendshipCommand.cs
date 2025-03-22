using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Application.Features.Friendships.Commands;

public class UpdateFriendshipCommand : IRequest<CommandResultDto<FriendshipDto>>
{
    public Guid Id { get; set; }
    public FriendshipStatus Status { get; set; }
}
