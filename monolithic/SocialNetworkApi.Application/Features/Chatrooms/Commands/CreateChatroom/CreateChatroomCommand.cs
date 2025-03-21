using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class CreateChatroomCommand : IRequest<CommandResultDto<ChatroomDto>>
{
    public string ChatroomName { get; set; } = string.Empty;
    public List<Guid> ParticipantIds { get; set; } = new List<Guid>();
}
