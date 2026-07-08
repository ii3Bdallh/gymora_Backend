using Domain.Enum;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Application.DTO.Base.Auditable;

namespace Application.DTO.CRUD.Update
{
    public record PlanPriceUDTO : BaseUDTO
    {
        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Amount { get; set; }
    }
}

