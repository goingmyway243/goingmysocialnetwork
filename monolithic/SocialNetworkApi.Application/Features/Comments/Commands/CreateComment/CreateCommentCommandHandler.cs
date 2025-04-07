using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Comments.Commands;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommandResultDto<CommentDto>>
{
    private readonly IRepository<CommentEntity> _commentRepository;
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IMapper _mapper;

    public CreateCommentCommandHandler(IRepository<CommentEntity> commentRepository, IRepository<PostEntity> postRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _mapper = mapper;
    }
    public async Task<CommandResultDto<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == default || request.PostId == default)
        {
            return CommandResultDto<CommentDto>.Failure("Invalid data!");
        }

        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post == null)
        {
            return CommandResultDto<CommentDto>.Failure("Post not found!");
        }

        var comment = _mapper.Map<CommentEntity>(request);
        comment.Id = Guid.NewGuid();

        await _commentRepository.InsertAsync(comment);

        post.CommentCount++;
        await _postRepository.UpdateAsync(post);

        var result = _mapper.Map<CommentDto>(comment);
        return CommandResultDto<CommentDto>.Success(result);
    }
}
