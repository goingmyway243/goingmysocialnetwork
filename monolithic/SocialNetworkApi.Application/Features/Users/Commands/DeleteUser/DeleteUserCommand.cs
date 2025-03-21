using System;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class DeleteUserCommand : IRequest<CommandResultDto<Guid>>
{
    public Guid Id { get; set; }
}
