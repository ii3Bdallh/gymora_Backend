namespace Application.DTO.Base.Auditable
{
    public record BaseAuditableCDTO : BaseCDTO
    {
        public int CreatedById { get; set; }
    }
}
