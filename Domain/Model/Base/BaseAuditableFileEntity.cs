using Domain.Attributes;
using Domain.Interface;
using Domain.Model.Auth;

namespace Domain.Model.Base
{
    /// <summary>
    /// Base class for auditable entities that contain files
    /// </summary>
    public abstract class BaseAuditableFileEntity : BaseFileEntity , IAuditableEntity
    {

        [Filterable(FilterType.Between)]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        [Filterable(FilterType.Exact)]

        public int CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }


    }
}