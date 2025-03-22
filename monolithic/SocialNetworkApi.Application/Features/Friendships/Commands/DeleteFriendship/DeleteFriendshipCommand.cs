using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Friendships.Commands;

public class DeleteFriendshipCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id;
}
