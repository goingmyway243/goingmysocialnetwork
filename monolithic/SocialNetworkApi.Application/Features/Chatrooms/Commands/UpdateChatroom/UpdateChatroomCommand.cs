using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class UpdateChatroomCommand : IRequest<CommandResult<ChatroomDto>>
{
    public Guid Id { get; set; }
    public string ChatroomName { get; set; } = string.Empty;
}
