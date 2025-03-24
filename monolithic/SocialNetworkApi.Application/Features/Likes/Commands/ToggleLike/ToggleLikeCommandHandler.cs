using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, CommandResultDto<int>>
{
    private readonly IRepository<LikeEntity> _likeRepository;
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IMapper _mapper;

    public ToggleLikeCommandHandler(
        IRepository<LikeEntity> likeRepository,
        IRepository<PostEntity> postRepository,
        IMapper mapper)
    {
        _likeRepository = likeRepository;
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<int>> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == default || request.PostId == default)
        {
            return CommandResultDto<int>.Failure("Invalid data.");
        }

        var existingPost = await _postRepository.GetByIdAsync(request.PostId);
        if (existingPost == null)
        {
            return CommandResultDto<int>.Failure("The current post is unavailable.");
        }

        var existingLikeInPost = await _likeRepository
            .FirstOrDefaultAsync(l => l.UserId == request.UserId && l.PostId == request.PostId);

        if (existingLikeInPost != null)
        {
            if (!request.IsLiked)
            {
                await UnlikePost(existingPost, existingLikeInPost);
            }

            return CommandResultDto<int>.Success(existingPost.LikeCount);
        }

        if (request.IsLiked)
        {
            await LikePost(request, existingPost);
        }

        return CommandResultDto<int>.Success(existingPost.LikeCount);
    }

    private async Task UnlikePost(PostEntity existingPost, LikeEntity existingLikeInPost)
    {
        existingPost.LikeCount--;
        await _postRepository.UpdateAsync(existingPost);
        await _likeRepository.DeleteAsync(existingLikeInPost);
    }

    private async Task LikePost(ToggleLikeCommand request, PostEntity existingPost)
    {
        var likeInPost = _mapper.Map<LikeEntity>(request);
        likeInPost.Id = Guid.NewGuid();

        existingPost.LikeCount++;
        await _postRepository.UpdateAsync(existingPost);
        await _likeRepository.InsertAsync(likeInPost);
    }
}
