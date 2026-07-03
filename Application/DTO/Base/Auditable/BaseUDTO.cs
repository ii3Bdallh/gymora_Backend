namespace Application.DTO.Base.Auditable
{
    public record BaseAuditableUDTO : BaseUDTO
    {
        public int CreatedById { get; set; }
    }
}
