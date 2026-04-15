using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.Post.API.Controllers;

[ApiController]
[Route("api/posts/{postId}/comments")]
[Authorize]
public class CommentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetComments(string postId)
    {
        var comments = await mediator.Send(new GetCommentsByPostIdQuery(postId));
        return Ok(comments);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> AddComment(string postId, [FromBody] AddCommentRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            var username = User.FindFirst("name")?.Value ?? "unknown";
            var comment = await mediator.Send(new AddCommentCommand(postId, userId, username, request.Content));
            return CreatedAtAction(nameof(GetComments), new { postId }, comment);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{commentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateComment(string postId, string commentId, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            var comment = await mediator.Send(new UpdateCommentCommand(commentId, userId, request.Content));
            return Ok(comment);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeleteComment(string postId, string commentId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            await mediator.Send(new DeleteCommentCommand(commentId, postId, userId));
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public record AddCommentRequest(string Content);
public record UpdateCommentRequest(string Content);
