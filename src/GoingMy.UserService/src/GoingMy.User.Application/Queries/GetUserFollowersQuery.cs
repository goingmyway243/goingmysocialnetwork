using GoingMy.User.Application.Commands;
using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

public record GetUserFollowersQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<IEnumerable<UserProfileDto>>;

public class GetUserFollowersQueryHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<GetUserFollowersQuery, IEnumerable<UserProfileDto>>
{
    public async Task<IEnumerable<UserProfileDto>> Handle(GetUserFollowersQuery request, CancellationToken cancellationToken)
    {
        var followers = await userProfileRepository.GetFollowersAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        return followers.Select(CreateUserProfileCommandHandler.MapToDto);
    }
}
