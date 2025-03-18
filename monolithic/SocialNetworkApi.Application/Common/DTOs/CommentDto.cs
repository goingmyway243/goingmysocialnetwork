namespace SocialNetworkApi.Application.Common.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
}
