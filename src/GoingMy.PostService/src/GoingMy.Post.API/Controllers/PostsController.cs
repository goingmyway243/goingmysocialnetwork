using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.Post.API.Controllers;

/// <summary>
/// Request DTO for creating a post through the API.
/// </summary>
public record CreatePostRequest(string Title, string Content);

/// <summary>
/// Request DTO for updating a post through the API.
/// </summary>
public record UpdatePostRequest(string Title, string Content);

/// <summary>
/// API controller for managing posts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new instance of the PostsController.
    /// </summary>
    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all posts for the authenticated user.
    /// </summary>
    /// <returns>A list of all posts.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetPosts()
    {
        var userId = User.FindFirst("sub")?.Value ?? "unknown";
        var username = User.FindFirst("name")?.Value ?? "unknown";

        var posts = await _mediator.Send(new GetPostsQuery());

        return Ok(new { userId, username, posts });
    }

    /// <summary>
    /// Retrieves a specific post by ID.
    /// </summary>
    /// <param name="id">The ID of the post to retrieve.</param>
    /// <returns>The requested post.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetPostById(string id)
    {
        var post = await _mediator.Send(new GetPostByIdQuery(id));

        if (post is null)
        {
            return NotFound($"Post with ID {id} not found");
        }

        return Ok(post);
    }

    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="request">The request containing post title and content.</param>
    /// <returns>The created post.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "unknown";
        var username = User.FindFirst("name")?.Value ?? "unknown";

        var command = new CreatePostCommand(request.Title, request.Content, userId, username);
        var newPost = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetPostById), new { id = newPost.Id }, new { message = "Post created successfully", post = newPost });
    }

    /// <summary>
    /// Updates an existing post.
    /// </summary>
    /// <param name="id">The ID of the post to update.</param>
    /// <param name="request">The request containing updated post title and content.</param>
    /// <returns>The updated post.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdatePost(string id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            var command = new UpdatePostCommand(id, request.Title, request.Content, userId);
            var updatedPost = await _mediator.Send(command);

            return Ok(new { message = "Post updated successfully", post = updatedPost });
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

    /// <summary>
    /// Deletes a post.
    /// </summary>
    /// <param name="id">The ID of the post to delete.</param>
    /// <returns>Success message.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeletePost(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            var command = new DeletePostCommand(id, userId);
            var result = await _mediator.Send(command);

            return Ok(new { message = $"Post {id} deleted successfully" });
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
