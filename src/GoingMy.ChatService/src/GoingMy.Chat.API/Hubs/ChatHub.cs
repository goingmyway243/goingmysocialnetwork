using GoingMy.Chat.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GoingMy.Chat.API.Hubs;

/// <summary>
/// SignalR hub for real-time chat messaging.
/// Clients join conversation groups to receive live message broadcasts.
/// </summary>
[Authorize]
public class ChatHub(IMediator mediator) : Hub
{
    /// <summary>
    /// Joins the SignalR group for a specific conversation.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Leaves the SignalR group for a specific conversation.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Sends a message to the conversation and broadcasts to all participants.
    /// </summary>
    public async Task SendMessage(string conversationId, string content)
    {
        var userId = Context.User?.FindFirstValue("sub")
            ?? throw new HubException("Unauthorized.");
        var username = Context.User?.FindFirstValue("name") ?? userId;

        try
        {
            var message = await mediator.Send(new SendMessageCommand(
                ConversationId: conversationId,
                SenderId: userId,
                SenderUsername: username,
                Content: content
            ));

            await Clients.Group(conversationId).SendAsync("ReceiveMessage", message);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
    }
}
