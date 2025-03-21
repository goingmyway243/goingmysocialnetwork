using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class DeletePostCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id { get; set; }
}
