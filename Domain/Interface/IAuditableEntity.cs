namespace Domain.Interface
{
    public interface IAuditableEntity
    {
        DateTime CreatedOn { get; set; }
        int CreatedById { get; set; }
    }
}