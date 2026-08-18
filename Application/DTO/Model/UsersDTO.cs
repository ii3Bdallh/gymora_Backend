
using Application.DTO.Base;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Model
{
    public record ApplicationUserCDTO : BaseCDTO
    {
        [Required]
        [MaxLength(100)]
        public string PersonName { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }


    }

    public record ApplicationUserUDTO : BaseUDTO
    {
        [MaxLength(100)]
        public string? PersonName { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

    }

    public record ApplicationUserRDTO : BaseRDTO
    {
        public string PersonName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}