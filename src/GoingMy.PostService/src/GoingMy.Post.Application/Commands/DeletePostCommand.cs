using GoingMy.Post.Infrastructure.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Commands;

/// <summary>
/// Command to delete a post.
/// </summary>
public record DeletePostCommand(
    int Id,
    string UserId
) : IRequest<bool>;

/// <summary>
/// Handler for the DeletePostCommand.
/// </summary>
public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IPostRepository _postRepository;

    public DeletePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Post with ID {request.Id} not found");

        // Verify ownership
        if (post.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts");
        }

        return await _postRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
