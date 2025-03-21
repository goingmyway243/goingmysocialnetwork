using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Contents.Commands;

public class DeleteContentCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id { get; set; }
}
