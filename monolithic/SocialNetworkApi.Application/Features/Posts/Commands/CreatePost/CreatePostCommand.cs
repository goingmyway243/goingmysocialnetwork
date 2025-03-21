using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class CreatePostCommand : IRequest<CommandResultDto<PostDto>>
{
    public string Caption { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? SharePostId { get; set; }
}
