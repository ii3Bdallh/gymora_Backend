using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO.Base;

namespace Application.DTO.Model
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

    public record SubscriptionPlanRDTO : BaseRDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int MaxOwnedGyms { get; set; }
        public int MaxCoachesPerGym { get; set; }
        public int MaxMembersPerGym { get; set; }
        public string? FeaturesJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }

        public ICollection<PlanPriceRDTO> Prices { get; set; } = new List<PlanPriceRDTO>();
    }

    public record SubscriptionPlanUDTO : BaseUDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int MaxOwnedGyms { get; set; }
        public int MaxCoachesPerGym { get; set; }
        public int MaxMembersPerGym { get; set; }
        public string? FeaturesJson { get; set; }
        public bool IsActive { get; set; }

        // public ICollection<PlanPriceUDTO> Prices { get; set; } = new List<PlanPriceUDTO>();
    }

}