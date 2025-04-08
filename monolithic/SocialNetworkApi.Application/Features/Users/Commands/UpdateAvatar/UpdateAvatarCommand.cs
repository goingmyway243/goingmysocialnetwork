using MediatR;
using Microsoft.AspNetCore.Http;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class UpdateAvatarCommand : IRequest<CommandResultDto<string>>
{
    public Guid UserId { get; set; }
    public IFormFile FormFile { get; set; } = null!;
}
