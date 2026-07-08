using Application.DTO.Base;
using Application.DTO.Base.Auditable;
namespace Application.DTO.CRUD.Read
{
    public record PlanPriceRDTO : BaseRDTO
    {
        public int PlanId { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

