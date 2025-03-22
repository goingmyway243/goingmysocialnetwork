using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Queries;

public class SearchUsersQuery : IRequest<PagedResultDto<UserDto>>
{
    public string SearchText { get; set; } = string.Empty;
    public bool IncludeFriendship { get; set; }
    public Guid? RequestUserId { get; set; }
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
