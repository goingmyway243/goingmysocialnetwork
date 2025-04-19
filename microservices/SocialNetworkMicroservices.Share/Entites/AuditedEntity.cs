namespace SocialNetworkMicroservices.Share.Entites;

public class AuditedEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }

    public AuditedEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
