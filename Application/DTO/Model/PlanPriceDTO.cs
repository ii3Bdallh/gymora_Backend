using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record PlanPriceCDTO
    {
        public int PlanId { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
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
        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Amount { get; set; }
    }
}