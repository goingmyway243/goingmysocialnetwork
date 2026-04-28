using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

public record CheckFollowStatusQuery(Guid FollowerId, Guid FolloweeId) : IRequest<bool>;

public class CheckFollowStatusQueryHandler(IUserFollowRepository userFollowRepository)
    : IRequestHandler<CheckFollowStatusQuery, bool>
{
    public async Task<bool> Handle(CheckFollowStatusQuery request, CancellationToken cancellationToken)
    {
        return await userFollowRepository.ExistsAsync(
            request.FollowerId, request.FolloweeId, cancellationToken);
    }
}
