using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Comments.Commands;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommandResultDto<CommentDto>>
{
    private readonly IRepository<CommentEntity> _commentRepository;
    private readonly IMapper _mapper;

    public UpdateCommentCommandHandler(IRepository<CommentEntity> commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<CommentDto>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.Id);
        if (comment == null)
        {
            return CommandResultDto<CommentDto>.Failure("Comment not found.");
        }

        comment.Comment = request.Comment;

        await _commentRepository.UpdateAsync(comment);

        var result = _mapper.Map<CommentDto>(comment);
        return CommandResultDto<CommentDto>.Success(result);
    }
}
