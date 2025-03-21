using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Chatrooms.Commands;

public class DeleteChatroomCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id { get; set; }
}
