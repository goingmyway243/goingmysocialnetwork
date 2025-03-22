using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Friendships.Commands;

public class CreateFriendshipCommand : IRequest<CommandResultDto<FriendshipDto>>
{
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
}
