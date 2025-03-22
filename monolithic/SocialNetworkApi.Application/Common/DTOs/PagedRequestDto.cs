namespace SocialNetworkApi.Application.Common.DTOs
{
    public class PagedRequestDto
    {
        public int PageSize = 10;
        public int PageIndex = 0;

        public int SkipCount => PageIndex * PageSize;
    }
}
