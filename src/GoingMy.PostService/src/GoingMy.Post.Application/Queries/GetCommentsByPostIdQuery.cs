using GoingMy.Post.Application.Commands;
using GoingMy.Post.Application.Dtos;
using GoingMy.Post.Domain.Repositories;
using MediatR;

namespace GoingMy.Post.Application.Queries;

public record GetCommentsByPostIdQuery(string PostId) : IRequest<IEnumerable<CommentDto>>;

public class GetCommentsByPostIdQueryHandler(ICommentRepository commentRepository)
    : IRequestHandler<GetCommentsByPostIdQuery, IEnumerable<CommentDto>>
{
    public async Task<IEnumerable<CommentDto>> Handle(GetCommentsByPostIdQuery request, CancellationToken cancellationToken)
    {
        var comments = await commentRepository.GetByPostIdAsync(request.PostId, cancellationToken);
        return comments.Select(AddCommentCommandHandler.MapToDto);
    }
}
