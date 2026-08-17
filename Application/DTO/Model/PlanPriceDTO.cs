using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record PlanPriceCDTO
    {
        public int PlanId { get; set; }

        [Required(ErrorMessage = "CountryCode is required.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "CountryCode must be exactly 2 characters.")]
        public string CountryCode { get; set; } = null!;

        [Required(ErrorMessage = "CurrencyCode is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CurrencyCode must be exactly 3 characters.")]
        public string CurrencyCode { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "DurationMonths must be at least 1.")]
        public int DurationMonths { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public decimal Amount { get; set; }
    }

    public record PlanPriceRDTO : BaseRDTO
    {
        public int PlanId { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public record PlanPriceUDTO : BaseUDTO
    {
        [StringLength(2, MinimumLength = 2, ErrorMessage = "CountryCode must be exactly 2 characters.")]
        public string CountryCode { get; set; } = null!;

        [StringLength(3, MinimumLength = 3, ErrorMessage = "CurrencyCode must be exactly 3 characters.")]
        public string CurrencyCode { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "DurationMonths must be at least 1.")]
        public int DurationMonths { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public decimal Amount { get; set; }
    }
}