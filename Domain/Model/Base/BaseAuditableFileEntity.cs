namespace Domain.Model.Base
{
    /// <summary>
    /// Base class for auditable entities that contain files
    /// </summary>
    public abstract class BaseAuditableFileEntity : BaseFileEntity
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }


    }
}