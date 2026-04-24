using GoingMy.Chat.Application.Commands;
using GoingMy.Chat.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoingMy.Chat.API.Controllers;

/// <summary>
/// Request DTO for starting or retrieving a conversation.
/// </summary>
public record CreateConversationRequest(string RecipientId, string RecipientUsername);

/// <summary>
/// Request DTO for sending a message.
/// </summary>
public record SendMessageRequest(string Content);

/// <summary>
/// Request DTO for editing a message.
/// </summary>
public record EditMessageRequest(string NewContent);

/// <summary>
/// API controller for managing conversations and messages.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves all conversations for the authenticated user.
    /// </summary>
    [HttpGet("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversations()
    {
        var userId = User.FindFirstValue("sub")!;
        var result = await mediator.Send(new GetConversationsQuery(userId));
        return Ok(result);
    }

    /// <summary>
    /// Creates a new conversation or returns the existing one between two users.
    /// </summary>
    [HttpPost("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = User.FindFirstValue("sub")!;
        var username = User.FindFirstValue("name") ?? userId;

        var result = await mediator.Send(new CreateConversationCommand(
            InitiatorId: userId,
            InitiatorUsername: username,
            RecipientId: request.RecipientId,
            RecipientUsername: request.RecipientUsername
        ));

        return Ok(result);
    }

    /// <summary>
    /// Retrieves paginated messages in a conversation.
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(string conversationId, [FromQuery] int pageNumber = 0, [FromQuery] int pageSize = 50)
    {
        var userId = User.FindFirstValue("sub")!;

        try
        {
            var result = await mediator.Send(new GetConversationMessagesQuery(conversationId, userId, pageNumber, pageSize));
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (InvalidOperationException) { return NotFound(); }
    }

    /// <summary>
    /// Sends a message in a conversation.
    /// </summary>
    [HttpPost("conversations/{conversationId}/messages")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(string conversationId, [FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirstValue("sub")!;
        var username = User.FindFirstValue("name") ?? userId;

        try
        {
            var result = await mediator.Send(new SendMessageCommand(
                ConversationId: conversationId,
                SenderId: userId,
                SenderUsername: username,
                Content: request.Content
            ));

            return CreatedAtAction(nameof(GetMessages), new { conversationId }, result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (InvalidOperationException) { return NotFound(); }
    }

    /// <summary>
    /// Soft-deletes a message. Only the sender may delete.
    /// </summary>
    [HttpDelete("conversations/{conversationId}/messages/{messageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMessage(string conversationId, string messageId)
    {
        var userId = User.FindFirstValue("sub")!;

        try
        {
            await mediator.Send(new DeleteMessageCommand(conversationId, messageId, userId));
            return NoContent();
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (InvalidOperationException) { return NotFound(); }
    }

    /// <summary>
    /// Edits a message's content. Only the sender may edit.
    /// </summary>
    [HttpPut("conversations/{conversationId}/messages/{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditMessage(string conversationId, string messageId, [FromBody] EditMessageRequest request)
    {
        var userId = User.FindFirstValue("sub")!;

        try
        {
            var result = await mediator.Send(new EditMessageCommand(conversationId, messageId, request.NewContent, userId));
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found")) { return NotFound(); }
        catch (InvalidOperationException) { return BadRequest(); }
    }

    /// <summary>
    /// Marks all messages in a conversation as read by the authenticated user.
    /// </summary>
    [HttpPost("conversations/{conversationId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(string conversationId)
    {
        var userId = User.FindFirstValue("sub")!;
        var username = User.FindFirstValue("name") ?? userId;

        try
        {
            var result = await mediator.Send(new MarkMessagesAsReadCommand(conversationId, userId, username));
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (InvalidOperationException) { return NotFound(); }
    }

    /// <summary>
    /// Retrieves all read receipts for a conversation.
    /// </summary>
    [HttpGet("conversations/{conversationId}/read-receipts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReadReceipts(string conversationId)
    {
        var userId = User.FindFirstValue("sub")!;

        try
        {
            var result = await mediator.Send(new GetReadReceiptsQuery(conversationId, userId));
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (InvalidOperationException) { return NotFound(); }
    }

    /// <summary>
    /// Searches messages in a conversation by content.
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchMessages(string conversationId, [FromQuery] string q, [FromQuery] int limit = 20)
    {
        var userId = User.FindFirstValue("sub")!;

        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search term cannot be empty.");

        try
        {
            var result = await mediator.Send(new SearchMessagesQuery(conversationId, userId, q, limit));
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (InvalidOperationException) { return NotFound(); }
    }
}
