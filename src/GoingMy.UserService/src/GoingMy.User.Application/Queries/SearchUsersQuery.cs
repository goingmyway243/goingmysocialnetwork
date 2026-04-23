using GoingMy.User.Application.Commands;
using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

public record SearchUsersQuery(
    string? SearchTerm,
    string? Location,
    bool? IsVerified,
    int Page = 1,
    int PageSize = 20,
    Guid? ExcludeUserId = null
) : IRequest<IEnumerable<UserProfileDto>>;

public class SearchUsersQueryHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<SearchUsersQuery, IEnumerable<UserProfileDto>>
{
    public async Task<IEnumerable<UserProfileDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userProfileRepository.SearchAsync(
            request.SearchTerm,
            request.Location,
            request.IsVerified,
            request.Page,
            request.PageSize,
            request.ExcludeUserId,
            cancellationToken);

        return users.Select(CreateUserProfileCommandHandler.MapToDto);
    }
}
