using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Comments.Commands;

public class CreateCommentCommand : IRequest<CommandResultDto<CommentDto>>
{
    public string Comment { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
}
