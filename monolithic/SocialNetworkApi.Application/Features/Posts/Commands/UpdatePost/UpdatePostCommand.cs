using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class UpdatePostCommand : IRequest<CommandResultDto<PostDto>>
{
    public Guid Id { get; set; }
    public string Caption { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}
