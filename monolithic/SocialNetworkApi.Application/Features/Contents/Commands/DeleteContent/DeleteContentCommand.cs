using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Contents.Commands;

public class DeleteContentCommand : IRequest<CommandResult<Guid>>
{
    public Guid Id { get; set; }
}
