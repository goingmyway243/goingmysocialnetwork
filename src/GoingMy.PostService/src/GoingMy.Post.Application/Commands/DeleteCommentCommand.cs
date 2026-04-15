using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Commands;

public record DeleteCommentCommand(string CommentId, string PostId, string UserId) : IRequest<bool>;

public class DeleteCommentCommandHandler(IPostRepository postRepository, ICommentRepository commentRepository)
    : IRequestHandler<DeleteCommentCommand, bool>
{
    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken)
            ?? throw new InvalidOperationException($"Comment '{request.CommentId}' not found.");

        if (comment.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        await commentRepository.DeleteAsync(request.CommentId, cancellationToken);
        await postRepository.DecrementCommentsAsync(request.PostId, cancellationToken);

        return true;
    }
}
