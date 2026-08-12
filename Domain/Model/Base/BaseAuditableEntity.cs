using Domain.Attributes;
using Domain.Interface;

namespace Domain.Model.Base;

public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTime CreatedOn { get; set; }
    [Filterable(FilterType.Exact)]

    public int CreatedById { get; set; }

}

