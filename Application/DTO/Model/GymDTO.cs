
using System.ComponentModel.DataAnnotations;
using Application.Common.FileValidation;
using Application.DTO.Base;
using Domain.Model.Base.Domain.Model.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Model
{
    public record GymCDTO : BaseAuditableFCDTO
    {

        [AllowedFileTypes(10, AllowedFileType.Jpg, AllowedFileType.Png)]
        public override IFormFile? File { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]

        public string? Email { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine1 cannot exceed 200 characters.")]
        public string? AddressLine1 { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine2 cannot exceed 200 characters.")]
        public string? AddressLine2 { get; set; }
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }
        [MaxLength(20, ErrorMessage = "PostalCode cannot exceed 20 characters.")]
        public string? PostalCode { get; set; }


        [Required(ErrorMessage = "Latitude is required.")]
        public decimal Latitude { get; set; }
        [Required(ErrorMessage = "Longitude is required.")]
        public decimal Longitude { get; set; }

        public GymStatus Status { get; set; } = GymStatus.Active;



    }

    public record GymUDTO : BaseAuditableFUDTO
    {
        [AllowedFileTypes(10, AllowedFileType.Jpg, AllowedFileType.Png)]
        public override IFormFile? File { get; set; }

        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine1 cannot exceed 200 characters.")]
        public string? AddressLine1 { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine2 cannot exceed 200 characters.")]
        public string? AddressLine2 { get; set; }
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }
        [MaxLength(20, ErrorMessage = "PostalCode cannot exceed 20 characters.")]
        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public GymStatus Status { get; set; }
    }

    public record GymRDTO : BaseAuditableFRDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public GymStatus Status { get; set; }

    }

    public sealed record SwitchGymDTO
    {
        [Required]
        public int GymId { get; init; }
    }

    public sealed record ChangeOwnerDTO
    {
        [Required]
        public int GymId { get; init; }

        [Required]
        public int NewOwnerUserId { get; init; }
    }
}
