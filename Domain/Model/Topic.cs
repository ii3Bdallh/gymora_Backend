using Domain.Attributes;
using Domain.Model.Base;

namespace Domain.Model
{
    public class Topic : BaseEntity
    {
        [Searchable]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SubjectId { get; set; }
    }
}