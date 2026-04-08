using GoingMy.User.Application.Commands;
using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

public record GetUserFollowingQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<IEnumerable<UserProfileDto>>;

public class GetUserFollowingQueryHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<GetUserFollowingQuery, IEnumerable<UserProfileDto>>
{
    public async Task<IEnumerable<UserProfileDto>> Handle(GetUserFollowingQuery request, CancellationToken cancellationToken)
    {
        var following = await userProfileRepository.GetFollowingAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        return following.Select(CreateUserProfileCommandHandler.MapToDto);
    }
}
