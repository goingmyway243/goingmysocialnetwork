using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

public record UserProfileSummaryDto(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    bool IsVerified);

/// <summary>
/// Retrieves lightweight user profile data for a set of user IDs.
/// Optimized for feed hydration (avatar + verification + names).
/// </summary>
public record GetUserProfilesByIdsQuery(IEnumerable<Guid> UserIds)
    : IRequest<IDictionary<Guid, UserProfileSummaryDto>>;

public class GetUserProfilesByIdsQueryHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<GetUserProfilesByIdsQuery, IDictionary<Guid, UserProfileSummaryDto>>
{
    public async Task<IDictionary<Guid, UserProfileSummaryDto>> Handle(
        GetUserProfilesByIdsQuery request,
        CancellationToken cancellationToken)
    {
        var profiles = await userProfileRepository.GetByIdsAsync(request.UserIds, cancellationToken);

        return profiles.ToDictionary(
            p => p.Id,
            p => new UserProfileSummaryDto(
                p.Id,
                p.Username,
                p.FirstName,
                p.LastName,
                p.AvatarUrl,
                p.IsVerified));
    }
}
