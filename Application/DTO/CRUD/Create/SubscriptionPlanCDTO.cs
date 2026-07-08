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
    public record SubscriptionPlanCDTO : BaseCDTO
    {
                public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int MaxOwnedGyms { get; set; }
        public int MaxCoachesPerGym { get; set; }
        public int MaxMembersPerGym { get; set; }
        public string? FeaturesJson { get; set; }

        public ICollection<PlanPriceCDTO> Prices { get; set; } = new List<PlanPriceCDTO>();
    }
}

