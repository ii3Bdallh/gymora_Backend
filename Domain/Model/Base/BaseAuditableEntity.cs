namespace Domain.Model.Base;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedOn { get; set; }
    public int CreatedById { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
