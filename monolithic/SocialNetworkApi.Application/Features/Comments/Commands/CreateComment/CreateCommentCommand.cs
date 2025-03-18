using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommand : IRequest<CommandResult<CommentDto>>
{
    public string Comment { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
}
