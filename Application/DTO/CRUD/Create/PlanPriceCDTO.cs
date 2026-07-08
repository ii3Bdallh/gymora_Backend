using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO.Base;
using Application.DTO.Base.Auditable;
namespace Application.DTO.CRUD.Create
{
    public record PlanPriceCDTO : BaseCDTO
    {
                public int PlanId { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Amount { get; set; }
    }
}

