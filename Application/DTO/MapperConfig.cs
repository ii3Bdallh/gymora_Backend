using Application.DTO.Base;
using Application.DTO.Model;
using AutoMapper;
using Domain.Model;
using Domain.Model.Auth;
using Domain.Model.Base;

namespace Application.DTO
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {



            // Script will Add After Here MapperConfig
            
            CreateMap<GymPerson, GymPersonRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap();

            CreateMap<GymPerson, GymPersonCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap();

            CreateMap<GymPerson, GymPersonUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap();

            CreateMap<GymStaffProfile, GymStaffProfileRDTO>().ReverseMap();
            CreateMap<GymStaffProfile, GymStaffProfileCDTO>().ReverseMap();
            CreateMap<GymStaffProfile, GymStaffProfileUDTO>().ReverseMap();

            CreateMap<GymMemberProfile, GymMemberProfileRDTO>().ReverseMap();
            CreateMap<GymMemberProfile, GymMemberProfileCDTO>().ReverseMap();
            CreateMap<GymMemberProfile, GymMemberProfileUDTO>().ReverseMap();

            CreateMap<Invitation, InvitationRDTO>()
                .IncludeBase<BaseGymEntity, BaseGymRDTO>()
                .ReverseMap();

            // Flatten Membership and Salary nested DTOs into Invitation entity
            CreateMap<InvitationCDTO, Invitation>()
                .IncludeBase<BaseGymCDTO, BaseGymEntity>()
                .ForMember(dest => dest.MembershipPlanId,  opt => opt.MapFrom(src => src.Membership != null ? src.Membership.MembershipPlanId : null))
                .ForMember(dest => dest.PlanName,          opt => opt.Ignore())
                .ForMember(dest => dest.DurationDays,      opt => opt.Ignore())
                .ForMember(dest => dest.Amount,            opt => opt.Ignore())
                .ForMember(dest => dest.DiscountAmount,    opt => opt.MapFrom(src => src.Membership != null ? (decimal?)src.Membership.DiscountAmount : null))
                .ForMember(dest => dest.FinalAmount,       opt => opt.Ignore())
                .ForMember(dest => dest.Salary,            opt => opt.MapFrom(src => src.Salary != null ? (decimal?)src.Salary.Salary : null))
                .ForMember(dest => dest.SalaryValidFrom,   opt => opt.MapFrom(src => src.Salary != null ? (DateTime?)src.Salary.SalaryValidFrom : null))
                .ForMember(dest => dest.SalaryValidUntil,  opt => opt.MapFrom(src => src.Salary != null ? (DateTime?)src.Salary.SalaryValidUntil : null));

            CreateMap<Invitation, InvitationUDTO>()
                .IncludeBase<BaseGymEntity, BaseGymUDTO>()
                .ReverseMap();


            



            CreateMap<ApplicationUser, ApplicationUserRDTO>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserCDTO>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserUDTO>().ReverseMap();

            CreateMap<Gym, GymRDTO>()
                .IncludeBase<BaseFileEntity, BaseFRDTO>()
                .ReverseMap();

            CreateMap<Gym, GymCDTO>()
                .IncludeBase<BaseFileEntity, BaseFCDTO>()
                .ReverseMap();

            CreateMap<Gym, GymUDTO>()
                .IncludeBase<BaseFileEntity, BaseFUDTO>()
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
            // Gym Base
            // ===========================

            CreateMap<BaseGymEntity, BaseGymRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ReverseMap();

            CreateMap<BaseGymEntity, BaseGymCDTO>()
                .IncludeBase<BaseEntity, BaseCDTO>()
                .ReverseMap();

            CreateMap<BaseGymEntity, BaseGymUDTO>()
                .IncludeBase<BaseEntity, BaseUDTO>()
                .ReverseMap();

            // ===========================
            // Gym Auditable
            // ===========================

            CreateMap<BaseAuditableGymEntity, BaseGymAuditableRDTO>()
                .IncludeBase<BaseGymEntity, BaseGymRDTO>()
                .ReverseMap();

            CreateMap<BaseAuditableGymEntity, BaseGymAuditableCDTO>()
                .IncludeBase<BaseGymEntity, BaseGymCDTO>()
                .ReverseMap();

            CreateMap<BaseAuditableGymEntity, BaseGymAuditableUDTO>()
                .IncludeBase<BaseGymEntity, BaseGymUDTO>()
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

            CreateMap<Attendance, AttendanceLogItemRDTO>()
                .IncludeBase<BaseEntity, BaseRDTO>()
                .ForMember(dest => dest.MemberFullName, opt => opt.MapFrom(src => src.Member.Name))
                .ForMember(dest => dest.DisplayId, opt => opt.MapFrom(src => $"#M-{src.MemberId}"))
                .ForMember(dest => dest.MembershipStatus, opt => opt.MapFrom(src => (src.Member.MemberProfile != null && src.Member.MemberProfile.MembershipEndDate.HasValue && src.Member.MemberProfile.MembershipEndDate.Value > DateTime.UtcNow) ? "Active" : "Expired"))
                .ForMember(dest => dest.RecordedByStaffName, opt => opt.MapFrom(src => src.RecordedBy != null ? src.RecordedBy.Name : null));

            CreateMap<RecordCheckInCDTO, Attendance>().ReverseMap();
            CreateMap<RecordCheckInUDTO, Attendance>().ReverseMap();

            CreateMap<MembershipPlan, MembershipPlanRDTO>()
                .IncludeBase<BaseAuditableGymEntity, BaseGymAuditableRDTO>()
                .ReverseMap();
            CreateMap<MembershipPlan, MembershipPlanCDTO>()
                .IncludeBase<BaseAuditableGymEntity, BaseGymAuditableCDTO>()
                .ReverseMap();
            CreateMap<MembershipPlan, MembershipPlanUDTO>()
                .IncludeBase<BaseAuditableGymEntity, BaseGymAuditableUDTO>()
                .ReverseMap();









        }
    }
}