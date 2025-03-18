using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class CreateLikeCommand : IRequest<CommandResult<LikeDto>>
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
}
