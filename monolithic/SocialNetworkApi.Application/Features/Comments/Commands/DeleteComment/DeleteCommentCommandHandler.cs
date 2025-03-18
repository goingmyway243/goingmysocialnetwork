using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Comments.Commands;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, CommandResult<Guid>>
{
    private readonly IRepository<CommentEntity> _commentRepository;

    public DeleteCommentCommandHandler(IRepository<CommentEntity> commentRepository)
    {
        _commentRepository = commentRepository;
    }
    public async Task<CommandResult<Guid>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.Id);
        if (comment == null)
        {
            return CommandResult<Guid>.Failure("Comment not found.");
        }

        await _commentRepository.DeleteAsync(comment);
        return CommandResult<Guid>.Success(comment.Id);
    }
}
