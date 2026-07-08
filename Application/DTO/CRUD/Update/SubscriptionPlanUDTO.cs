using Domain.Enum;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Application.DTO.Base.Auditable;

namespace Application.DTO.CRUD.Update
{
    public record SubscriptionPlanUDTO : BaseUDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int MaxOwnedGyms { get; set; }
        public int MaxCoachesPerGym { get; set; }
        public int MaxMembersPerGym { get; set; }
        public string? FeaturesJson { get; set; }
        public bool IsActive { get; set; }

        public ICollection<PlanPriceUDTO> Prices { get; set; } = new List<PlanPriceUDTO>();
    }
}

