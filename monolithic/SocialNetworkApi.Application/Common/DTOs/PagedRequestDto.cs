namespace SocialNetworkApi.Application.Common.DTOs
{
    public class PagedRequestDto
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public DateTime CursorTimestamp { get; set; }

        public int SkipCount => PageIndex * PageSize;
    }
}
