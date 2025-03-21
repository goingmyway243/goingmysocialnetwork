using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class UpdateChatMessageCommand : IRequest<CommandResultDto<ChatMessageDto>>
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
}
