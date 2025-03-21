using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, CommandResultDto<PostDto>>
{
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IMapper _mapper;

    public UpdatePostCommandHandler(IRepository<PostEntity> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<PostDto>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id);
        if (post == null)
        {
            return CommandResultDto<PostDto>.Failure("Post not found.");
        }

        post.Caption = request.Caption;
        post.CommentCount = request.CommentCount;
        post.LikeCount = request.LikeCount;

        await _postRepository.UpdateAsync(post);
        return CommandResultDto<PostDto>.Success(_mapper.Map<PostDto>(post));
    }
}
