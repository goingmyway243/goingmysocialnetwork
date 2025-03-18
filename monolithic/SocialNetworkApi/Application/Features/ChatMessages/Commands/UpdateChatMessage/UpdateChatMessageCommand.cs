using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class UpdateChatMessageCommand : IRequest<CommandResult<ChatMessageDto>>
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
}
