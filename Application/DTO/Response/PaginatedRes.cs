using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
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
