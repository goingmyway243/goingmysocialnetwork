using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommand : IRequest<CommandResult<CommentDto>>
{
    public Guid Id { get; set; }
    public string Comment { get; set; } = string.Empty;
}
