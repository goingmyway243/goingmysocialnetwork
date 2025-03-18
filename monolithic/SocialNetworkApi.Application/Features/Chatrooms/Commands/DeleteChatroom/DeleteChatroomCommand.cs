using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands.DeleteChatroom;

public class DeleteChatroomCommand : IRequest<CommandResult<Guid>>
{
    public Guid Id { get; set; }
}
