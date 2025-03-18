namespace Social.UserService.Domain.Common
{
  public class BaseEntity
  {
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public BaseEntity()
    {
      CreatedDate = DateTime.UtcNow;
      ModifiedDate = DateTime.UtcNow;
    }
  }
}
