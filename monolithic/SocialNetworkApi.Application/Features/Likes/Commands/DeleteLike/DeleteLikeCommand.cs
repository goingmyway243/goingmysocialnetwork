using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Likes.Commands;

public class DeleteLikeCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id { get; set; }
}
