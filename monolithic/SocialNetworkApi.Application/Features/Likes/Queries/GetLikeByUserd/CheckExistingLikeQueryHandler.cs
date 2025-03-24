using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Likes.Queries;

public class CheckExistingLikeQueryHandler : IRequestHandler<CheckExistingLikeQuery, QueryResultDto<bool>>
{
    private readonly IRepository<LikeEntity> _likeRepository;

    public CheckExistingLikeQueryHandler(IRepository<LikeEntity> likeRepository)
    {
        _likeRepository = likeRepository;
    }

    public async Task<QueryResultDto<bool>> Handle(CheckExistingLikeQuery request, CancellationToken cancellationToken)
    {
        var existingLikeInPost = await _likeRepository
            .FirstOrDefaultAsync(l => l.UserId == request.UserId && l.PostId == request.PostId);

        return QueryResultDto<bool>.Success(existingLikeInPost != null);
    }
}
