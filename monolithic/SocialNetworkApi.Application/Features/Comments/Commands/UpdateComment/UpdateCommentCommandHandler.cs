using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommandResult<CommentDto>>
{
    private readonly IRepository<CommentEntity> _commentRepository;
    private readonly IMapper _mapper;

    public UpdateCommentCommandHandler(IRepository<CommentEntity> commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<CommandResult<CommentDto>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.Id);
        if (comment == null)
        {
            return CommandResult<CommentDto>.Failure("Comment not found.");
        }

        comment.Comment = request.Comment;

        await _commentRepository.UpdateAsync(comment);

        var result = _mapper.Map<CommentDto>(comment);
        return CommandResult<CommentDto>.Success(result);
    }
}
