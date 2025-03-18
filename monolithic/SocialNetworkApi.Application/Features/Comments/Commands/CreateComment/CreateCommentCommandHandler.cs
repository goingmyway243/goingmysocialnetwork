using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommandResult<CommentDto>>
{
    private readonly IRepository<CommentEntity> _commentRepository;
    private readonly IMapper _mapper;

    public CreateCommentCommandHandler(IRepository<CommentEntity> commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }
    public async Task<CommandResult<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == default || request.PostId == default)
        {
            return CommandResult<CommentDto>.Failure("Invalid data!");
        }

        var comment = _mapper.Map<CommentEntity>(request);
        comment.Id = Guid.NewGuid();

        await _commentRepository.InsertAsync(comment);

        var result = _mapper.Map<CommentDto>(request);
        return CommandResult<CommentDto>.Success(result);
    }
}
