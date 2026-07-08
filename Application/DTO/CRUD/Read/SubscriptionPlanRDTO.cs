using Application.DTO.Base;
using Application.DTO.Base.Auditable;
namespace Application.DTO.CRUD.Read
{
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
}

