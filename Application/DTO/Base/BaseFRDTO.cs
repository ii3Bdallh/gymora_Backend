namespace Application.DTO.Base
{
    public record BaseFRDTO : BaseRDTO
    {
        public string FileUrl { get; set; } = string.Empty;
    }
}