using Domain.Attributes;
using Domain.Interface;

namespace Domain.Model.Base
{
    /// <summary>
    /// Base class for auditable entities that contain files
    /// </summary>
    public abstract class BaseAuditableFileEntity : BaseFileEntity , IAuditableEntity
    {

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    [Filterable(FilterType.Exact)]

        public int CreatedById { get; set; }


    }
}