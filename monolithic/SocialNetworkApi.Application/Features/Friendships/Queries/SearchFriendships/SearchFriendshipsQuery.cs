using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Application.Features.Friendships.Queries;

public class SearchFriendshipsQuery : IRequest<PagedResultDto<FriendshipDto>>
{
    public Guid UserId { get; set; }
    public List<FriendshipStatus> FilterStatus { get; set; } = new List<FriendshipStatus>();
    public bool ExcludeFriendshipMakeByUser { get; set; }
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
