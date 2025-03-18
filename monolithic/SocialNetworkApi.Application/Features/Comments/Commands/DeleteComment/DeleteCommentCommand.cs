using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Comments.Commands;

public class DeleteCommentCommand : IRequest<CommandResult<Guid>>
{
    public Guid Id { get; set; }
}
