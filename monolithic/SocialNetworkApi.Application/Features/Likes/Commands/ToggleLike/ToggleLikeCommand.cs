using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class ToggleLikeCommand : IRequest<CommandResultDto<int>>
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public bool IsLiked { get; set; }
}
