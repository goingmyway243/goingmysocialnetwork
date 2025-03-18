using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class DeleteLikeCommand : IRequest<CommandResult<Guid>>
{
    public Guid Id { get; set; }
}
