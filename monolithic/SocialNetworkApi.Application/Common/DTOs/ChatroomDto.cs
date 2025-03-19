namespace SocialNetworkApi.Application.Common.DTOs;

public class ChatroomDto
{
    public Guid Id { get; set; }
    public string ChatroomName { get; set; } = string.Empty;
    public List<UserDto> Participants { get; set; } = new List<UserDto>();
}
