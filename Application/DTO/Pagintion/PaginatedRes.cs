
namespace Application.DTO.Pagintion
{
    public class PaginatedRes<T>
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; } 
        public int PageNumber { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
        public IEnumerable<T> Items { get; set; } = [];
    }
}
