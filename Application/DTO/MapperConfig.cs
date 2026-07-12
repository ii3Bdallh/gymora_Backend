
using Application.DTO.CRUD.Create;
using Application.DTO.CRUD.Read;
using Application.DTO.CRUD.Update;
using AutoMapper;
using Domain.Model;

namespace Application.DTO
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {

            // Script will Add After Here MapperConfig
            CreateMap<SubscriptionPlan, SubscriptionPlanCDTO>()
                .ReverseMap();
            CreateMap<SubscriptionPlan, SubscriptionPlanUDTO>()
                .ReverseMap();
            CreateMap<SubscriptionPlan, SubscriptionPlanRDTO>()
                .ReverseMap();
                
            CreateMap<PlanPrice, PlanPriceCDTO>()
                .ReverseMap();
            CreateMap<PlanPrice, PlanPriceUDTO>()
                .ReverseMap();
            CreateMap<PlanPrice, PlanPriceRDTO>()
                .ReverseMap();











        }
    }
}