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
    /// Retrieves all messages in a conversation.
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(string conversationId)
    {
        var userId = User.FindFirstValue("sub")!;

        try
        {
            var result = await mediator.Send(new GetConversationMessagesQuery(conversationId, userId));
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
}
