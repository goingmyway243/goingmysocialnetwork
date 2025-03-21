using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class DeleteChatMessageCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id { get; set; }
}
