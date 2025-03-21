using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Posts.Queries;

public class SearchPostsQuery : IRequest<PagedResultDto<PostDto>>
{
    public Guid? OwnerId { get; set; }
    public string? SearchText { get; set; }
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
