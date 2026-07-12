using System.ComponentModel.DataAnnotations;
using Application.Common.FileValidation;
using Application.DTO.Base.Auditable;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.TrainerCertificate;

public record TrainerCertificateCDTO : BaseAuditableFCDTO
{
    [Required]
    [AllowedFileTypes(5, AllowedFileType.Pdf, AllowedFileType.Jpg, AllowedFileType.Png)]
    public override IFormFile File { get; set; } = null!;

    [Required]
    public int TrainerId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}

public record TrainerCertificateUDTO : BaseAuditableFUDTO
{
    [AllowedFileTypes(5, AllowedFileType.Pdf, AllowedFileType.Jpg, AllowedFileType.Png)]
    public override IFormFile? File { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}

public record TrainerCertificateRDTO : BaseAuditableFRDTO
{
    public int TrainerId { get; set; }
    public string Title { get; set; } = string.Empty;
}
