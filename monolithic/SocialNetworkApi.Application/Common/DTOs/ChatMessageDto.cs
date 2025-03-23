namespace SocialNetworkApi.Application.Common.DTOs;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid ChatroomId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }

    public UserDto? User { get; set; }
}
