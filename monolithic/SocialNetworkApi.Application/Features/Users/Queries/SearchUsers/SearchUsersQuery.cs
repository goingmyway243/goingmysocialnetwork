using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Queries;

public class SearchUsersQuery : IRequest<PagedResultDto<UserDto>>
{
    public string SearchText { get; set; } = string.Empty;
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
