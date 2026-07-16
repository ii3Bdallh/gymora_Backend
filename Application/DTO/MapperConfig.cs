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
            
            CreateMap<Users, UsersRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap();

            CreateMap<Users, UsersCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap();

            CreateMap<Users, UsersUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap();


            CreateMap<Gym, GymRDTO>()
                .IncludeBase<BaseAuditableFileEntity, BaseAuditableFRDTO>()
                .ReverseMap();

            CreateMap<Gym, GymCDTO>()
                .IncludeBase<BaseAuditableFileEntity, BaseAuditableFCDTO>()
                .ReverseMap();

            CreateMap<Gym, GymUDTO>()
                .IncludeBase<BaseAuditableFileEntity, BaseAuditableFUDTO>()
                .ReverseMap();


            
            CreateMap<CouponRedemption, CouponRedemptionRDTO>()
                .IncludeBase<BaseAuditableEntity, BaseAuditableRDTO>()
                .ReverseMap();

            CreateMap<CouponRedemption, CouponRedemptionCDTO>()
                .IncludeBase<BaseAuditableEntity, BaseAuditableCDTO>()
                .ReverseMap();

            CreateMap<CouponRedemption, CouponRedemptionUDTO>()
                .IncludeBase<BaseAuditableEntity, BaseAuditableUDTO>()
                .ReverseMap();

                
            
            CreateMap<OwnerSubscription, OwnerSubscriptionRDTO>()
                .IncludeBase<BaseAuditableEntity, BaseAuditableRDTO>()
                .ReverseMap();

            CreateMap<OwnerSubscription, OwnerSubscriptionCDTO>()
                .IncludeBase<BaseAuditableEntity, BaseAuditableCDTO>()
                .ReverseMap();

            CreateMap<OwnerSubscription, OwnerSubscriptionUDTO>()
                .IncludeBase<BaseAuditableEntity, BaseAuditableUDTO>()
                .ReverseMap();

                
            
            CreateMap<Coupon, CouponRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap();

            CreateMap<Coupon, CouponCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap();

            CreateMap<Coupon, CouponUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap();


            CreateMap<PaymentRequest, PaymentRequestRDTO>()
                .IncludeBase<BaseAuditableFileEntity, BaseAuditableFRDTO>()
                .ReverseMap();

            CreateMap<PaymentRequest, PaymentRequestCDTO>()
                .IncludeBase<BaseAuditableFileEntity, BaseAuditableFCDTO>()
                .ReverseMap();

            CreateMap<PaymentRequest, PaymentRequestUDTO>()
                .IncludeBase<BaseAuditableFileEntity, BaseAuditableFUDTO>()
                .ReverseMap();


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

            CreateMap<BaseAuditableEntity, BaseAuditableRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap();

            CreateMap<BaseAuditableEntity, BaseAuditableCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap();

            CreateMap<BaseAuditableEntity, BaseAuditableUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap();


            // ===========================
            // File
            // ===========================

            CreateMap<BaseFileEntity, BaseFRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap()
                .ForMember(x => x.StoredFilePath, opt => opt.Ignore());

            CreateMap<BaseFileEntity, BaseFCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFilePath, opt => opt.Ignore());

            CreateMap<BaseFileEntity, BaseFUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFilePath, opt => opt.Ignore());


            // ===========================
            // Auditable File
            // ===========================

            CreateMap<BaseAuditableFileEntity, BaseAuditableFRDTO>()
                .IncludeBase<BaseFileEntity, BaseFRDTO>()
                .ReverseMap()
                .ForMember(x => x.StoredFilePath, opt => opt.Ignore());

            CreateMap<BaseAuditableFileEntity, BaseAuditableFCDTO>()
                .IncludeBase<BaseFileEntity, BaseFCDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFilePath, opt => opt.Ignore());

            CreateMap<BaseAuditableFileEntity, BaseAuditableFUDTO>()
                .IncludeBase<BaseFileEntity, BaseFUDTO>()
                .ReverseMap()
                .ForMember(x => x.FileUrl, opt => opt.Ignore())
                .ForMember(x => x.StoredFilePath, opt => opt.Ignore());
            #endregion








        }
    }
}