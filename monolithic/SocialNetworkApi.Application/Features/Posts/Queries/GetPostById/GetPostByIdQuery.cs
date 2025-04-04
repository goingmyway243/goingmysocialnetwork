using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Posts.Queries;

public class GetPostByIdQuery : IRequest<QueryResultDto<PostDto>>
{
    public Guid Id { get; set; }
}
