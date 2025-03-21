using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Comments.Commands;

public class DeleteCommentCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id { get; set; }
}
