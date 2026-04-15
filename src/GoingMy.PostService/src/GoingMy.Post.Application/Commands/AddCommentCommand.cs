using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Entities;
using GoingMy.Post.Domain.Repositories;
using MassTransit;
using MediatR;

namespace GoingMy.Post.Application.Commands;

public record AddCommentCommand(string PostId, string UserId, string Username, string Content)
    : IRequest<CommentDto>;

public class AddCommentCommandHandler(IPostRepository postRepository, ICommentRepository commentRepository)
    : IRequestHandler<AddCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        _ = await postRepository.GetByIdAsync(request.PostId, cancellationToken)
            ?? throw new InvalidOperationException($"Post '{request.PostId}' not found.");

        var comment = new Comment(
            id: NewId.NextSequentialGuid().ToString(),
            postId: request.PostId,
            userId: request.UserId,
            username: request.Username,
            content: request.Content,
            createdAt: DateTime.UtcNow);

        await commentRepository.AddAsync(comment, cancellationToken);
        await postRepository.IncrementCommentsAsync(request.PostId, cancellationToken);

        return MapToDto(comment);
    }

    internal static CommentDto MapToDto(Comment c) =>
        new(c.Id, c.PostId, c.UserId, c.Username, c.Content, c.CreatedAt, c.UpdatedAt);
}
