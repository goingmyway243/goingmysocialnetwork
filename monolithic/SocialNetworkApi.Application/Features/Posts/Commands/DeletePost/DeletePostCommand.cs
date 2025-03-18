using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class DeletePostCommand : IRequest<CommandResult<Guid>>
{
    public Guid Id { get; set; }
}
