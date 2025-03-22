using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Chatrooms.Queries;

public class SearchChatroomsQuery : IRequest<PagedResultDto<ChatroomDto>>
{
    public string SearchText { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
