namespace Application.DTO.Base.Auditable
{
    public record BaseAuditableFRDTO : BaseAuditableRDTO
    {
        public string FileUrl { get; set; } = string.Empty;
    }
}
