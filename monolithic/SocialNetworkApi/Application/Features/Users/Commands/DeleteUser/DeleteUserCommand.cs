using System;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class DeleteUserCommand : IRequest<CommandResult<Guid>>
{
    public Guid UserId { get; set; }
}
