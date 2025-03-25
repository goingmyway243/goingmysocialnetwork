using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Comments.Queries;

public class SearchCommentsQuery : IRequest<PagedResultDto<CommentDto>>
{
    public Guid PostId { get; set; }
    public PagedRequestDto PagedRequest { get; set; } = new PagedRequestDto();
}
