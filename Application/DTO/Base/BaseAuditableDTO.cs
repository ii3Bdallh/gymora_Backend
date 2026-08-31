namespace Application.DTO.Base
{
    public record BaseAuditableCDTO : BaseCDTO
    {
    }

    public record BaseAuditableUDTO : BaseUDTO
    {
    }
    public record BaseAuditableRDTO : BaseRDTO
    {

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }



    }
}