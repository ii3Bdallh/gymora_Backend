using Domain.Model.Auth;

namespace Domain.Interface
{
    public interface IAuditableEntity
    {
        DateTime CreatedOn { get; set; }
        int CreatedById { get; set; }
        ApplicationUser? CreatedBy { get; set; }
    }
}