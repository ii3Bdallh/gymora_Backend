using Domain.Attributes;
using Domain.Interface;
using Domain.Model.Auth;

namespace Domain.Model.Base;

public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    [Filterable(FilterType.Between)]
    public DateTime CreatedOn { get; set; }
    [Filterable(FilterType.Exact)]

    public int CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }



}

