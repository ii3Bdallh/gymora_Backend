namespace Application.DTO.Base
{
    public record BaseAuditableCDTO : BaseCDTO
    {
        public int CreatedById { get; set; }
    }

    public record BaseAuditableUDTO : BaseUDTO
    {
        public int CreatedById { get; set; }
    }
    public record BaseAuditableRDTO : BaseRDTO
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }



    }
}