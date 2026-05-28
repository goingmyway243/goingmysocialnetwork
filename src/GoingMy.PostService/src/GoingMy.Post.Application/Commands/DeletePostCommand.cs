using GoingMy.Post.Domain.Repositories;
using GoingMy.Shared.Events;
using MassTransit;
using MediatR;

namespace GoingMy.Post.Application.Commands;

/// <summary>
/// Command to delete a post.
/// </summary>
public record DeletePostCommand(
    string Id,
    string UserId
) : IRequest<bool>;

/// <summary>
/// Handler for the DeletePostCommand.
/// </summary>
public class DeletePostCommandHandler(IPostRepository postRepository, IPublishEndpoint publishEndpoint)
    : IRequestHandler<DeletePostCommand, bool>
{
    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Post with ID {request.Id} not found");

        // Verify ownership
        if (post.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts");
        }

        var deleted = await postRepository.DeleteAsync(request.Id, cancellationToken);

        if (deleted)
        {
            await publishEndpoint.Publish(new PostDeletedEvent
            {
                PostId = request.Id,
                UserId = request.UserId,
                DeletedAt = DateTime.UtcNow
            }, cancellationToken);

            foreach (var media in post.MediaAttachments ?? [])
            {
                await publishEndpoint.Publish(new FileOrphanedEvent
                {
                    FileId = media.FileId
                }, cancellationToken);
            }
        }

        return deleted;
    }
}
