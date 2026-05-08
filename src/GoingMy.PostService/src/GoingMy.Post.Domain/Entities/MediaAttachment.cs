namespace GoingMy.Post.Domain.Entities;

public class MediaAttachment
{
    public string FileId { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public int? Width { get; set; }
    public int? Height { get; set; }
}
