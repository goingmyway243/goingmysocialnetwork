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
        var (userId, username) = GetCallerIdentity();

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
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException)
        {
            throw new HubException(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a message and notifies all conversation participants.
    /// </summary>
    public async Task DeleteMessage(string conversationId, string messageId)
    {
        var (userId, _) = GetCallerIdentity();

        try
        {
            await mediator.Send(new DeleteMessageCommand(conversationId, messageId, userId));
            await Clients.Group(conversationId).SendAsync("MessageDeleted", new { messageId, deletedBy = userId });
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException)
        {
            throw new HubException(ex.Message);
        }
    }

    /// <summary>
    /// Edits a message and broadcasts the updated message to all participants.
    /// </summary>
    public async Task EditMessage(string conversationId, string messageId, string newContent)
    {
        var (userId, _) = GetCallerIdentity();

        try
        {
            var updated = await mediator.Send(new EditMessageCommand(conversationId, messageId, newContent, userId));
            await Clients.Group(conversationId).SendAsync("MessageEdited", updated);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException)
        {
            throw new HubException(ex.Message);
        }
    }

    /// <summary>
    /// Marks all messages in a conversation as read and broadcasts receipts.
    /// </summary>
    public async Task MarkAsRead(string conversationId)
    {
        var (userId, username) = GetCallerIdentity();

        try
        {
            var receipts = await mediator.Send(new MarkMessagesAsReadCommand(conversationId, userId, username));
            await Clients.Group(conversationId).SendAsync("MessagesRead", receipts);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException)
        {
            throw new HubException(ex.Message);
        }
    }

    /// <summary>
    /// Broadcasts a typing indicator to all other participants in the conversation.
    /// </summary>
    public async Task SendTypingIndicator(string conversationId)
    {
        var (userId, username) = GetCallerIdentity();
        await Clients.OthersInGroup(conversationId).SendAsync("UserTyping", new { userId, username });
    }

    /// <summary>
    /// Broadcasts a "stopped typing" indicator to all other participants.
    /// </summary>
    public async Task SendStoppedTyping(string conversationId)
    {
        var (userId, _) = GetCallerIdentity();
        await Clients.OthersInGroup(conversationId).SendAsync("UserStoppedTyping", new { userId });
    }

    private (string UserId, string Username) GetCallerIdentity()
    {
        var userId = Context.User?.FindFirstValue("sub")
            ?? throw new HubException("Unauthorized.");
        var username = Context.User?.FindFirstValue("name") ?? userId;
        return (userId, username);
    }
}
