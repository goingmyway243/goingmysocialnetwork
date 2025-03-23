using MediatR;
using Microsoft.AspNetCore.SignalR;
using SocialNetworkApi.Application.Features.ChatMessages.Commands;

namespace SocialNetworkApi.Hubs;

public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task SendMessage(CreateChatMessageCommand message)
    {
        var result = await _mediator.Send(message);
        if (result.IsSuccess)
        {
            await Clients.All.SendAsync("ReceiveMessage", result.Data);
        }
    }
}
