namespace SocialNetworkApi.Application.Common.DTOs
{
    public class PagedResultDto<T>
    {
        public List<T>? Items { get; set; }
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public int TotalCount { get; set; } 
        public int PageIndex { get; set; }

        public PagedResultDto(List<T>? items, bool success = false, string error = "") {
            Items = items;
            IsSuccess = success;
            Error = error;
        }

        public PagedResultDto<T> WithPage(int pageIndex, int totalCount)
        {
            TotalCount = totalCount;
            PageIndex = pageIndex;

            return this;
        }

        public static PagedResultDto<T> Success(List<T> data)
        {
            return new PagedResultDto<T>(data, true);
        }

        public static PagedResultDto<T> Failure(string error)
        {
            return new PagedResultDto<T>(default, false, "Query failed: " + error);
        }
    }
}
