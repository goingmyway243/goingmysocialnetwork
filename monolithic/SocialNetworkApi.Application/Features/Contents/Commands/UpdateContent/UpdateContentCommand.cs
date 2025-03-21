using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Contents.Commands;

public class UpdateContentCommand : IRequest<CommandResultDto<ContentDto>>
{
    public Guid Id { get; set; }
    public string TextContent { get; set; } = string.Empty;
}
