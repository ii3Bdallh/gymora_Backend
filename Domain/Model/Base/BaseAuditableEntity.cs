using Domain.Interface;

namespace Domain.Model.Base;

public abstract class BaseAuditableEntity : BaseEntity , IAuditableEntity
{
    public DateTime CreatedOn { get; set; }
    public int CreatedById { get; set; }

}

