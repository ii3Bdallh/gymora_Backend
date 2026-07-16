using Application.DTO.Filters;

namespace Application.DTO.Pagintion
{
    public class PaginatedSearchReq
    {
        const int maxPageSize = 50;
        const int maxSearchTermLength = 100;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > maxPageSize ? maxPageSize : value;
        }

        public int PageNumber { get; set; } = 1;

        private string? _searchTerm;
        public string? SearchTerm
        {
            get => _searchTerm;
            set => _searchTerm = value?.Length > maxSearchTermLength
                ? value[..maxSearchTermLength]
                : value;
        }

        public string? OrderBy { get; set; } = "Id";
        public string OrderDirection { get; set; } = "asc";

        public bool IsActive { get; set; } = true;


        public FilterRequest? Filters { get; set; }
    }
}
