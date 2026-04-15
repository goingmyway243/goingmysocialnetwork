using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Commands;

public record UpdateCommentCommand(string CommentId, string UserId, string Content)
    : IRequest<CommentDto>;

public class UpdateCommentCommandHandler(ICommentRepository commentRepository)
    : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken)
            ?? throw new InvalidOperationException($"Comment '{request.CommentId}' not found.");

        if (comment.UserId != request.UserId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        comment.Update(request.Content);
        await commentRepository.UpdateAsync(comment, cancellationToken);

        return AddCommentCommandHandler.MapToDto(comment);
    }
}
