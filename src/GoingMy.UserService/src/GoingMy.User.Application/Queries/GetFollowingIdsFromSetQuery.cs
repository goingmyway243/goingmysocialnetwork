using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

/// <summary>
/// Given a set of candidate user IDs, returns the subset that the caller is following.
/// Useful for bulk follow-status checks after a search.
/// </summary>
public record GetFollowingIdsFromSetQuery(Guid FollowerId, IEnumerable<Guid> CandidateIds) : IRequest<IEnumerable<Guid>>;

public class GetFollowingIdsFromSetQueryHandler(IUserFollowRepository userFollowRepository)
    : IRequestHandler<GetFollowingIdsFromSetQuery, IEnumerable<Guid>>
{
    public async Task<IEnumerable<Guid>> Handle(GetFollowingIdsFromSetQuery request, CancellationToken cancellationToken)
    {
        return await userFollowRepository.GetFollowingIdsFromSetAsync(
            request.FollowerId, request.CandidateIds, cancellationToken);
    }
}
