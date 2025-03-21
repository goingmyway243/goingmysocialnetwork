using Microsoft.AspNetCore.Http;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Application.Common.DTOs;

public class ContentDto
{
    public Guid Id { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string LinkContent { get; set; } = string.Empty;
    public ContentType Type { get; set; }
    public Guid PostId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }

    public IFormFile? FormFile { get; set; }
}
