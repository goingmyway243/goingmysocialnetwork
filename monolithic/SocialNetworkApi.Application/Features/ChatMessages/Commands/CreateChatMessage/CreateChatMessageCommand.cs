using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.ChatMessages.Commands;

public class CreateChatMessageCommand : IRequest<CommandResultDto<ChatMessageDto>>
{
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid ChatroomId { get; set; }
}
