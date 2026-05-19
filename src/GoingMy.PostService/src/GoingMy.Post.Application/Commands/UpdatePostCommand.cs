using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;

namespace GoingMy.Post.Application.Commands;

/// <summary>
/// Command to update an existing post.
/// </summary>
public record UpdatePostCommand(
    string Id,
    string Content,
    string UserId
) : IRequest<PostDto>;

/// <summary>
/// Handler for the UpdatePostCommand.
/// </summary>
public class UpdatePostCommandHandler(IPostRepository postRepository, IPublishEndpoint publishEndpoint)
    : IRequestHandler<UpdatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Post with ID {request.Id} not found");

        // Verify ownership
        if (post.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only update your own posts");
        }

        post.Update(request.Content);
        var updatedPost = await postRepository.UpdateAsync(post, cancellationToken);

        await publishEndpoint.Publish(new PostUpdatedEvent
        {
            PostId = updatedPost.Id,
            UserId = updatedPost.UserId,
            Content = updatedPost.Content,
            MediaAttachments = updatedPost.MediaAttachments?.Select(m => m.Url).ToList() ?? [],
            UpdatedAt = updatedPost.UpdatedAt ?? DateTime.UtcNow
        }, cancellationToken);

        return CreatePostCommandHandler.MapToDto(updatedPost);
    }
}
