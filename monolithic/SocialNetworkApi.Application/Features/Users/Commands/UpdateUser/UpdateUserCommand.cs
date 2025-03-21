using MediatR;
using SocialNetworkApi.Application.Common.DTOs;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<CommandResultDto<UserDto>>
{
    public Guid Id { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? FullName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
}
