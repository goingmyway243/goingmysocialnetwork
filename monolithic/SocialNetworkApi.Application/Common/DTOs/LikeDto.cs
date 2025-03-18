namespace SocialNetworkApi.Application.Common.DTOs
{
    public class LikeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
    }
}
