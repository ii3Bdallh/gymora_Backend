using Application.DTO.Base;
using Application.DTO.Model;
using AutoMapper;
using Domain.Model;
using Domain.Model.Base;

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
                .ReverseMap()
                .ForMember(dest => dest.Prices, opt => opt.Ignore())
                ;
            CreateMap<SubscriptionPlan, SubscriptionPlanRDTO>()
                .ReverseMap();

            CreateMap<PlanPrice, PlanPriceCDTO>()
                .ReverseMap();
            CreateMap<PlanPrice, PlanPriceUDTO>()
                .ReverseMap();
            CreateMap<PlanPrice, PlanPriceRDTO>()
                .ReverseMap();





            #region Base Mappings
            // ===========================
            // Base
            // ===========================

            CreateMap<BaseEntity, BaseRDTO>()
                .ReverseMap();

            CreateMap<BaseEntity, BaseCDTO>()
                .ReverseMap();

            CreateMap<BaseEntity, BaseUDTO>()
                .ReverseMap();


            // ===========================
            // Auditable
            // ===========================

            CreateMap<AuditableEntity, BaseAuditableRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap();

            CreateMap<AuditableEntity, BaseAuditableCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap();

            CreateMap<AuditableEntity, BaseAuditableUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap();


            // ===========================
            // File
            // ===========================

            CreateMap<BaseFileEntity, BaseFRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap()
                .ForMember(x => x.StoredFileName, opt => opt.Ignore());

            CreateMap<BaseFileEntity, BaseFCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFileName, opt => opt.Ignore());

            CreateMap<BaseFileEntity, BaseFUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFileName, opt => opt.Ignore());


            // ===========================
            // Auditable File
            // ===========================

            CreateMap<BaseAuditableFileEntity, BaseAuditableFRDTO>()
                .IncludeBase<AuditableEntity, BaseAuditableRDTO>()
                .ReverseMap()
                .ForMember(x => x.StoredFileName, opt => opt.Ignore());

            CreateMap<BaseAuditableFileEntity, BaseAuditableFCDTO>()
                .IncludeBase<AuditableEntity, BaseAuditableCDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFileName, opt => opt.Ignore());

            CreateMap<BaseAuditableFileEntity, BaseAuditableFUDTO>()
                .IncludeBase<AuditableEntity, BaseAuditableUDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFileName, opt => opt.Ignore());
            #endregion








        }
    }
}