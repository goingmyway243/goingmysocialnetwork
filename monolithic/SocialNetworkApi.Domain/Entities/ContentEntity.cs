using SocialNetworkApi.Domain.Common;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Domain.Entities;

public class ContentEntity : AuditedEntity
{
    public string TextContent { get; set; } = string.Empty;
    public string LinkContent { get; set; } = string.Empty;
    public ContentType Type { get; set; }
    public Guid PostId { get; set; }

    // Relationship
    public PostEntity Post { get; set; } = null!;
}
