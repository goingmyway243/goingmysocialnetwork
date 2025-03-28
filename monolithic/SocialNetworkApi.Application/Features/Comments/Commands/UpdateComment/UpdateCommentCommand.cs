using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Comments.Commands;

public class UpdateCommentCommand : IRequest<CommandResultDto<CommentDto>>
{
    public Guid Id { get; set; }
    public string Comment { get; set; } = string.Empty;
}
