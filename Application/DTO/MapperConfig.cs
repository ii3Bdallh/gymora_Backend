
using Application.DTO.CRUD.Create;
using Application.DTO.CRUD.Read;
using Application.DTO.CRUD.Update;
using Application.DTO.TrainerCertificate;
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

            CreateMap<Domain.Model.TrainerCertificate, TrainerCertificateCDTO>()
                .ForMember(dest => dest.File, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
                .ForMember(dest => dest.StoredFileName, opt => opt.Ignore());

            CreateMap<Domain.Model.TrainerCertificate, TrainerCertificateUDTO>()
                .ForMember(dest => dest.File, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
                .ForMember(dest => dest.StoredFileName, opt => opt.Ignore());

            CreateMap<Domain.Model.TrainerCertificate, TrainerCertificateRDTO>()
                .ReverseMap();
                

                










        }
    }
}