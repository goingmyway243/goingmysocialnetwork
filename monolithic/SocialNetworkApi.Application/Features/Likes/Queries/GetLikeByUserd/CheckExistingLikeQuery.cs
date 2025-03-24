using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Likes.Queries;

public class CheckExistingLikeQuery : IRequest<QueryResultDto<bool>>
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
}
