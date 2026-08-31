# Extracted Module: Subscriptions, Owner Subscriptions & Payment Requests

تم استخراج جميع المكونات وتصنيفها حسب طبقات الـ Clean Architecture (Domain, Application, Infrastructure, Api) لنقلها بسهولة إلى مشروعك الجديد.

---

## 🏗️ Structure Overview

```
├── Domain/
│   ├── Model/
│   │   ├── SubscriptionPlan.cs
│   │   ├── PlanPrice.cs
│   │   ├── OwnerSubscription.cs
│   │   └── PaymentRequest.cs
│   ├── Enum/
│   │   ├── OwnerSubscriptionStatus.cs
│   │   ├── PaymentRequestStatus.cs
│   │   └── PaymentMethod.cs
│   └── Events/
│       ├── PaymentEvent.cs
│       └── SubscriptionActivatedEvent.cs
├── Application/
│   ├── DTO/
│   │   └── Model/
│   │       ├── SubscriptionPlanDTO.cs
│   │       ├── PlanPriceDTO.cs
│   │       ├── OwnerSubscriptionDTO.cs
│   │       └── PaymentRequestDTO.cs
│   ├── Interface/
│   │   ├── Repo/
│   │   │   ├── ISubscriptionPlanRepo.cs
│   │   │   ├── IOwnerSubscriptionRepo.cs
│   │   │   └── IPaymentRequestRepo.cs
│   │   └── Service/
│   │       ├── ISubscriptionPlanService.cs
│   │       ├── IOwnerSubscriptionService.cs
│   │       ├── IPaymentRequestService.cs
│   │       └── ICurrentPlanService.cs
│   ├── Service/
│   │   ├── SubscriptionPlanService.cs
│   │   ├── OwnerSubscriptionService.cs
│   │   ├── PaymentRequestService.cs
│   │   └── CurrentPlanService.cs
│   └── EventConsumer/
│       └── PaymentRequestApprovedConsumer.cs
├── Infrastructure/
│   ├── Configurations/
│   │   ├── SubscriptionPlanConfig.cs
│   │   ├── PlanPriceConfig.cs
│   │   ├── OwnerSubscriptionConfig.cs
│   │   └── PaymentRequestConfig.cs
│   └── Repo/
│       ├── SubscriptionPlanRepo.cs
│       ├── OwnerSubscriptionRepo.cs
│       └── PaymentRequestRepo.cs
└── Api/
    └── Controllers/
        ├── SubscriptionPlanController.cs
        ├── OwnerSubscriptionController.cs
        └── PaymentRequestController.cs
```

---

## 1. 📦 Domain Layer

### 1.1 Entities

#### `SubscriptionPlan.cs`
```csharp
using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Base;
using System;
using System.Collections.Generic;

namespace Domain.Model
{
    public class SubscriptionPlan : BaseEntity, ICacheableEntity
    {
        [Searchable]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        [Filterable(FilterType.Exact)]
        public bool IsFree { get; set; }

        [Filterable(FilterType.Between)]
        public int MaxOwnedGyms { get; set; }

        [Filterable(FilterType.Between)]
        public int MaxCoaches { get; set; }

        [Filterable(FilterType.Between)]
        public int MaxMembers { get; set; }

        public string? FeaturesJson { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime CreatedOn { get; set; }

        public ICollection<PlanPrice> Prices { get; set; } = new List<PlanPrice>();
    }
}
```

#### `PlanPrice.cs`
```csharp
using Domain.Attributes;
using Domain.Enum;
using Domain.Model.Base;
using System;

namespace Domain.Model
{
    public class PlanPrice : BaseEntity
    {
        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;

        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
```

#### `OwnerSubscription.cs`
```csharp
using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Model
{
    public class OwnerSubscription : BaseAuditableEntity, IOnlyMeCanSee
    {
        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;

        public int PlanPriceId { get; set; }
        public PlanPrice PlanPrice { get; set; } = null!;

        public int? PaymentRequestId { get; set; }
        public PaymentRequest? PaymentRequest { get; set; }

        [Filterable(FilterType.Between)]
        public decimal AmountPaid { get; set; }

        [Filterable(FilterType.Exact)]
        public string CurrencyCode { get; set; } = null!;

        [Filterable(FilterType.Between)]
        public DateTime StartDate { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime EndDate { get; set; }

        [NotMapped]
        public OwnerSubscriptionStatus Status
        {
            get
            {
                var now = DateTime.UtcNow;
                if (now <= EndDate)
                    return OwnerSubscriptionStatus.Active;

                return OwnerSubscriptionStatus.Expired;
            }
        }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
```

#### `PaymentRequest.cs`
```csharp
using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Auth;
using Domain.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class PaymentRequest : BaseAuditableFileEntity, IOnlyMeCanSee
    {
        [Searchable]
        public string? CouponCode { get; set; }

        [Filterable(FilterType.Between)]
        public decimal OriginalAmount { get; set; }

        [Filterable(FilterType.Between)]
        public decimal DiscountAmount { get; set; }

        [Filterable(FilterType.Between)]
        public decimal FinalAmount { get; set; }

        [Filterable(FilterType.Exact)]
        public string CurrencyCode { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public PaymentRequestStatus Status { get; set; } = PaymentRequestStatus.Pending;

        public string? ReviewNotes { get; set; }
        public string? RejectionReason { get; set; }

        [Filterable(FilterType.Exact)]
        public int? ReviewedBy { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }

        [Filterable(FilterType.Between)]
        public DateTime? ReviewedAt { get; set; }

        [Filterable(FilterType.Exact)]
        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public int PlanPriceId { get; set; }
        public PlanPrice PlanPrice { get; set; } = null!;

        [Filterable(FilterType.Exact)]
        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        public virtual ICollection<OwnerSubscription> OwnerSubscriptions { get; set; } = new List<OwnerSubscription>();

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
```

---

### 1.2 Enums

#### `OwnerSubscriptionStatus.cs`
```csharp
namespace Domain.Enum;

public enum OwnerSubscriptionStatus
{
    Active = 1,
    Expired = 3,
    Suspended = 4
}
```

#### `PaymentRequestStatus.cs`
```csharp
namespace Domain.Enum
{
    public enum PaymentRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }
}
```

#### `PaymentMethod.cs`
```csharp
namespace Domain.Enum
{
    public enum PaymentMethod
    {
        Cash = 0,
        BankTransfer = 1,
        Instapay = 2,
        Wallet = 3,
        Other = 4
    }
}
```

---

### 1.3 Events

#### `PaymentEvent.cs`
```csharp
namespace Domain.Events;

public record PaymentCreatedEvent(int PaymentRequestId);

public record PaymentApprovedEvent(int PaymentRequestId, int UserId, int? CouponId, decimal? DiscountAmount);

public record PaymentRejectedEvent(int PaymentRequestId, int UserId, string RejectionReason);
```

#### `SubscriptionActivatedEvent.cs`
```csharp
namespace Domain.Events;

public record SubscriptionActivatedEvent(int SubscriptionId, int PaymentRequestId, int OwnerUserId);
```

---

## 2. ⚙️ Application Layer

### 2.1 DTOs

#### `SubscriptionPlanDTO.cs`
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Domain.Model;

namespace Application.DTO.Model
{
    public record SubscriptionPlanCDTO : BaseCDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;
        public bool IsFree { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxOwnedGyms must be non-negative.")]
        public int MaxOwnedGyms { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxCoaches must be non-negative.")]
        public int MaxCoaches { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxMembers must be non-negative.")]
        public int MaxMembers { get; set; }
        public string? FeaturesJson { get; set; }

        public ICollection<PlanPriceCDTO> Prices { get; set; } = new List<PlanPriceCDTO>();
    }

    public record SubscriptionPlanRDTO : BaseRDTO
    {
        public string Name { get; set; } = null!;
        public bool IsFree { get; set; }
        public string? Description { get; set; }
        public int MaxOwnedGyms { get; set; }
        public int MaxCoaches { get; set; }
        public int MaxMembers { get; set; }
        public string? FeaturesJson { get; set; }
        public DateTime CreatedOn { get; set; }

        public ICollection<PlanPriceRDTO> Prices { get; set; } = new List<PlanPriceRDTO>();
    }

    public record SubscriptionPlanUDTO : BaseUDTO
    {
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;
        public bool IsFree { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxOwnedGyms must be non-negative.")]
        public int MaxOwnedGyms { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxCoaches must be non-negative.")]
        public int MaxCoaches { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxMembers must be non-negative.")]
        public int MaxMembers { get; set; }
        public string? FeaturesJson { get; set; }
    }
}
```

#### `PlanPriceDTO.cs`
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record PlanPriceCDTO
    {
        public int PlanId { get; set; }

        [Required(ErrorMessage = "CountryCode is required.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "CountryCode must be exactly 2 characters.")]
        public string CountryCode { get; set; } = null!;

        [Required(ErrorMessage = "CurrencyCode is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CurrencyCode must be exactly 3 characters.")]
        public string CurrencyCode { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "DurationMonths must be at least 1.")]
        public int DurationMonths { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public decimal Amount { get; set; }
    }

    public record PlanPriceRDTO : BaseRDTO
    {
        public int PlanId { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public int DurationMonths { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public record PlanPriceUDTO : BaseUDTO
    {
        [StringLength(2, MinimumLength = 2, ErrorMessage = "CountryCode must be exactly 2 characters.")]
        public string CountryCode { get; set; } = null!;

        [StringLength(3, MinimumLength = 3, ErrorMessage = "CurrencyCode must be exactly 3 characters.")]
        public string CurrencyCode { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "DurationMonths must be at least 1.")]
        public int DurationMonths { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
        public decimal Amount { get; set; }
    }
}
```

#### `OwnerSubscriptionDTO.cs`
```csharp
using System;
using Application.DTO.Base;
using Domain.Enum;

namespace Application.DTO.Model
{
    public record OwnerSubscriptionCDTO : BaseAuditableCDTO
    {
        public int PlanId { get; set; }
        public int PlanPriceId { get; set; }
        public int? PaymentRequestId { get; set; }
        public decimal AmountPaid { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public record OwnerSubscriptionUDTO : BaseAuditableUDTO
    {
        public DateTime EndDate { get; set; }
    }

    public record OwnerSubscriptionRDTO : BaseAuditableRDTO
    {
        public int PlanId { get; set; }
        public int PlanPriceId { get; set; }
        public int? PaymentRequestId { get; set; }
        public decimal AmountPaid { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OwnerSubscriptionStatus Status { get; set; }

        public SubscriptionPlanRDTO? Plan { get; set; }
        public PlanPriceRDTO? PlanPrice { get; set; }
        public PaymentRequestRDTO? PaymentRequest { get; set; }
    }

    public class CurrentPlanResult
    {
        public int PlanId { get; init; }
        public string PlanName { get; init; } = null!;
        public int MaxOwnedGyms { get; init; }
        public int MaxMembers { get; init; }
        public int MaxCoaches { get; init; }
        public bool IsFree { get; init; }
        public OwnerSubscriptionStatus? SubscriptionStatus { get; init; }
        public OwnerSubscriptionRDTO? Subscription { get; init; }

        public int CurrentGymCount { get; init; }
        public int CurrentMemberCount { get; init; }
        public int CurrentCoachCount { get; init; }

        public bool IsOverGymLimit => CurrentGymCount > MaxOwnedGyms;
        public bool IsOverMemberLimit => CurrentMemberCount > MaxMembers;
        public bool IsOverCoachLimit => CurrentCoachCount > MaxCoaches;

        public bool IsCompliant => !IsOverGymLimit && !IsOverMemberLimit && !IsOverCoachLimit;
    }
}
```

#### `PaymentRequestDTO.cs`
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Application.Common.FileValidation;
using Application.DTO.Base;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Application.DTO.Model
{
    public record PaymentRequestCDTO : BaseAuditableFCDTO
    {
        [Required(ErrorMessage = "Payment proof is required.")]
        [AllowedFileTypes(5, AllowedFileType.Jpg, AllowedFileType.Png)]
        public override IFormFile? File { get; set; }

        public int PlanId { get; set; }
        public int PlanPriceId { get; set; }

        [BindNever]
        public int? CouponId { get; set; }
        public string? CouponCode { get; set; }

        [BindNever]
        public decimal OriginalAmount { get; set; }
        [BindNever]
        public decimal DiscountAmount { get; set; }
        [BindNever]
        public decimal FinalAmount { get; set; }
        [BindNever]
        public string? CurrencyCode { get; set; }
    }

    public record PaymentRequestUDTO : BaseAuditableFUDTO
    {
        [BindNever]
        public override IFormFile? File { get; set; }
        public PaymentRequestStatus Status { get; set; }
        public string? ReviewNotes { get; set; }
        public string? RejectionReason { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public record PaymentRequestApprove
    {
        public string? ReviewNotes { get; set; }
    }

    public record PaymentRequestReject
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        public string RejectionReason { get; set; } = null!;
    }

    public record PaymentRequestRDTO : BaseAuditableFRDTO
    {
        public int PlanId { get; set; }
        public int PlanPriceId { get; set; }
        public int? SubscriptionId { get; set; }
        public int? CouponId { get; set; }
        public string? CouponCode { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public string? ProofUrl { get; set; }
        public PaymentRequestStatus Status { get; set; }
        public string? ReviewNotes { get; set; }
        public string? RejectionReason { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public SubscriptionPlanRDTO? Plan { get; set; }
        public PlanPriceRDTO? PlanPrice { get; set; }
        public CouponRDTO? Coupon { get; set; }
    }
}
```

---

### 2.2 AutoMapper Profile Snippet (`MapperConfig.cs`)

```csharp
// OwnerSubscription
CreateMap<OwnerSubscription, OwnerSubscriptionRDTO>()
    .IncludeBase<BaseAuditableEntity, BaseAuditableRDTO>()
    .ReverseMap();

CreateMap<OwnerSubscription, OwnerSubscriptionCDTO>()
    .IncludeBase<BaseAuditableEntity, BaseAuditableCDTO>()
    .ReverseMap();

CreateMap<OwnerSubscription, OwnerSubscriptionUDTO>()
    .IncludeBase<BaseAuditableEntity, BaseAuditableUDTO>()
    .ReverseMap();

// PaymentRequest
CreateMap<PaymentRequest, PaymentRequestRDTO>()
    .IncludeBase<BaseAuditableFileEntity, BaseAuditableFRDTO>()
    .ForMember(dest => dest.ProofUrl, opt => opt.MapFrom(src => src.FileUrl))
    .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.OwnerSubscriptions.OrderByDescending(s => s.CreatedOn).Select(s => s.Id).Cast<int?>().FirstOrDefault()))
    .ReverseMap();

CreateMap<PaymentRequest, PaymentRequestCDTO>()
    .IncludeBase<BaseAuditableFileEntity, BaseAuditableFCDTO>()
    .ReverseMap();

CreateMap<PaymentRequest, PaymentRequestUDTO>()
    .IncludeBase<BaseAuditableFileEntity, BaseAuditableFUDTO>()
    .ReverseMap();

// SubscriptionPlan & PlanPrice
CreateMap<SubscriptionPlan, SubscriptionPlanCDTO>().ReverseMap();
CreateMap<SubscriptionPlan, SubscriptionPlanUDTO>()
    .ReverseMap()
    .ForMember(dest => dest.Prices, opt => opt.Ignore())
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
CreateMap<SubscriptionPlan, SubscriptionPlanRDTO>().ReverseMap();

CreateMap<PlanPrice, PlanPriceCDTO>().ReverseMap();
CreateMap<PlanPrice, PlanPriceUDTO>()
    .ReverseMap()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
CreateMap<PlanPrice, PlanPriceRDTO>().ReverseMap();
```

---

### 2.3 Repository Interfaces

#### `ISubscriptionPlanRepo.cs`
```csharp
using Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface ISubscriptionPlanRepo : IBaseRepo<SubscriptionPlan>
    {
        Task<PlanPrice> AddPlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default);
        Task<PlanPrice> DeletePlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default);
        Task<PlanPrice?> GetPlanPriceByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default);
        Task<SubscriptionPlan?> GetFreePlanAsync(CancellationToken ct = default);
    }
}
```

#### `IOwnerSubscriptionRepo.cs`
```csharp
using Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IOwnerSubscriptionRepo : IBaseRepo<OwnerSubscription>
    {
        Task<OwnerSubscription?> GetCurrentSubscriptionAsync(int ownerUserId, CancellationToken ct = default);
    }
}
```

#### `IPaymentRequestRepo.cs`
```csharp
using Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IPaymentRequestRepo : IBaseRepo<PaymentRequest>
    {
        Task<bool> HasPendingRequestAsync(int UserId, CancellationToken ct = default);
        Task<bool> HasUsedThisCouponBeforeAsync(int UserId, int CouponId, CancellationToken ct = default);
    }
}
```

---

### 2.4 Service Interfaces

#### `ISubscriptionPlanService.cs`
```csharp
using Application.DTO.Model;
using Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface ISubscriptionPlanService : IBaseService<SubscriptionPlan, SubscriptionPlanRDTO, SubscriptionPlanCDTO, SubscriptionPlanUDTO>
    {
        Task<PlanPriceRDTO> AddPlanPriceAsync(int PlanId, PlanPriceCDTO dto, CancellationToken cancellationToken = default);
        Task<PlanPriceRDTO> DeletePlanPriceAsync(int id, CancellationToken cancellationToken = default);
        Task<PlanPriceRDTO?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PlanPriceRDTO> UpdatePlanPriceAsync(int id, PlanPriceUDTO dto, CancellationToken cancellationToken = default);
    }
}
```

#### `IOwnerSubscriptionService.cs`
```csharp
using Application.DTO.Model;
using Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IOwnerSubscriptionService : IBaseService<OwnerSubscription, OwnerSubscriptionRDTO, OwnerSubscriptionCDTO, OwnerSubscriptionUDTO>
    {
        Task<OwnerSubscriptionRDTO> CreateFromApprovedPaymentAsync(int paymentRequestId, CancellationToken ct = default);
    }
}
```

#### `IPaymentRequestService.cs`
```csharp
using Application.DTO.Model;
using Domain.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IPaymentRequestService : IBaseService<PaymentRequest, PaymentRequestRDTO, PaymentRequestCDTO, PaymentRequestUDTO>
    {
        Task<PaymentRequestRDTO> ApproveAsync(int id, PaymentRequestApprove dto, CancellationToken ct = default);
        Task<PaymentRequestRDTO> RejectAsync(int id, PaymentRequestReject dto, CancellationToken ct = default);
    }
}
```

#### `ICurrentPlanService.cs`
```csharp
using Application.DTO.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service.Shared
{
    public interface ICurrentPlanService
    {
        Task<CurrentPlanResult> GetCurrentPlanAsync(int ownerUserId, CancellationToken ct = default);
    }
}
```

---

### 2.5 Service Implementations

#### `SubscriptionPlanService.cs`
```csharp
using Application.Cache;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Events;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.Entity
{
    public class SubscriptionPlanService : BaseService<SubscriptionPlan, SubscriptionPlanRDTO, SubscriptionPlanCDTO, SubscriptionPlanUDTO>, ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepo _subscriptionPlanRepo;

        public SubscriptionPlanService(
            ISubscriptionPlanRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<SubscriptionPlanService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _subscriptionPlanRepo = repo;
        }

        public async Task<PlanPriceRDTO> AddPlanPriceAsync(int PlanId, PlanPriceCDTO dto, CancellationToken cancellationToken = default)
        {
            SubscriptionPlan? entity = await _subscriptionPlanRepo.GetByIdAsync(PlanId, trackChanges: false, cancellationToken: cancellationToken);
            if (entity is null)
                throw new NotFoundException($"{typeof(SubscriptionPlan).Name} with ID {PlanId} was not found.");

            PlanPrice planPrice = _mapper.Map<PlanPrice>(dto);
            planPrice.PlanId = PlanId;

            planPrice = await _subscriptionPlanRepo.AddPlanPriceAsync(planPrice, cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<SubscriptionPlan>(), planPrice.PlanId, CurrentGymId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<PlanPriceRDTO>(planPrice);
        }

        public async Task<PlanPriceRDTO> DeletePlanPriceAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, trackChanges: true, cancellationToken: cancellationToken);
            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            await _subscriptionPlanRepo.DeletePlanPriceAsync(entity, cancellationToken);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<SubscriptionPlan>(), entity.PlanId, CurrentGymId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<PlanPriceRDTO>(entity);
        }

        public async Task<PlanPriceRDTO?> GetPlanPriceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, trackChanges: false, cancellationToken: cancellationToken);
            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            return _mapper.Map<PlanPriceRDTO>(entity);
        }

        public async Task<PlanPriceRDTO> UpdatePlanPriceAsync(int id, PlanPriceUDTO dto, CancellationToken cancellationToken = default)
        {
            PlanPrice? entity = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(id, trackChanges: true, cancellationToken: cancellationToken);
            if (entity is null)
                throw new NotFoundException($"{typeof(PlanPrice).Name} with ID {id} was not found.");

            _logger.LogInformation("Updating PlanPrice with ID {Id}", id);
            _mapper.Map(dto, entity);

            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<SubscriptionPlan>(), entity.PlanId, CurrentGymId),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<PlanPriceRDTO>(entity);
        }
    }
}
```

#### `OwnerSubscriptionService.cs`
```csharp
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Enum;
using Domain.Events;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class OwnerSubscriptionService : BaseAuditableService<OwnerSubscription, OwnerSubscriptionRDTO, OwnerSubscriptionCDTO, OwnerSubscriptionUDTO>, IOwnerSubscriptionService
    {
        private readonly IPaymentRequestRepo _paymentRequestRepo;
        private readonly IOwnerSubscriptionRepo _ownerSubscriptionRepo;
        private readonly ISubscriptionPlanRepo _subscriptionPlanRepo;
        private readonly ICurrentPlanService _currentPlanService;

        public OwnerSubscriptionService(
            IOwnerSubscriptionRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<OwnerSubscriptionService> logger,
            IPaymentRequestRepo paymentRequestRepo,
            ISubscriptionPlanRepo subscriptionPlanRepo,
            ICurrentPlanService currentPlanService)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _paymentRequestRepo = paymentRequestRepo;
            _ownerSubscriptionRepo = repo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _currentPlanService = currentPlanService;
        }

        public async Task<OwnerSubscriptionRDTO> CreateFromApprovedPaymentAsync(int paymentRequestId, CancellationToken ct = default)
        {
            var payment = await _paymentRequestRepo.GetByIdIgnoringSecurityAsync(paymentRequestId, false, ct);
            if (payment == null || payment.Status != PaymentRequestStatus.Approved)
                throw new BadRequestException("Payment request is not approved.");

            CurrentPlanResult existingSubscription = await _currentPlanService.GetCurrentPlanAsync(payment.CreatedById, ct);
            if (existingSubscription.IsFree == false)
                throw new ConflictException("User already has an active subscription.");

            PlanPrice? planPrice = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(payment.PlanPriceId, false, ct);
            if (planPrice == null)
                throw new NotFoundException("Invalid subscription plan price.");

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMonths(planPrice.DurationMonths);

            var subscription = new OwnerSubscription
            {
                CreatedById = payment.CreatedById,
                PlanId = payment.PlanId,
                PlanPriceId = payment.PlanPriceId,
                PaymentRequestId = payment.Id,
                AmountPaid = payment.FinalAmount,
                CurrencyCode = payment.CurrencyCode,
                StartDate = startDate,
                EndDate = endDate
            };

            await _repo.AddAsync(subscription, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var result = _mapper.Map<OwnerSubscriptionRDTO>(subscription);
            await _publishEndpoint.Publish(new SubscriptionActivatedEvent(result.Id, payment.Id, payment.CreatedById), ct);

            return result;
        }
    }
}
```

#### `PaymentRequestService.cs`
```csharp
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Enum;
using Domain.Events;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class PaymentRequestService : BaseAuditableFileService<PaymentRequest, PaymentRequestRDTO, PaymentRequestCDTO, PaymentRequestUDTO>, IPaymentRequestService
    {
        private readonly IPaymentRequestRepo _paymentRepo;
        private readonly ISubscriptionPlanRepo _subscriptionPlanRepo;
        private readonly ICouponService _couponService;
        private readonly IOwnerSubscriptionRepo _ownerSubscriptionRepo;
        private readonly ICurrentPlanService _currentPlanService;

        public PaymentRequestService(
            IPaymentRequestRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<PaymentRequestService> logger,
            IStorageService storageService,
            ISubscriptionPlanRepo subscriptionPlanRepo,
            ICouponService couponService,
            IOwnerSubscriptionRepo ownerSubscriptionRepo,
            ICurrentPlanService currentPlanService)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService, logger)
        {
            _paymentRepo = repo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
            _couponService = couponService;
            _ownerSubscriptionRepo = ownerSubscriptionRepo;
            _currentPlanService = currentPlanService;
        }

        public override async Task<PaymentRequestRDTO> AddAsync(PaymentRequestCDTO dto, CancellationToken ct = default)
        {
            if (await _paymentRepo.HasPendingRequestAsync(_currentUser.UserId, ct))
                throw new ConflictException("You already have a pending payment request.");

            PlanPrice? planPrice = await _subscriptionPlanRepo.GetPlanPriceByIdAsync(dto.PlanPriceId, false, ct);
            if (planPrice == null || planPrice.Plan == null || planPrice.Plan.IsFree == true)
                throw new BadRequestException("Invalid subscription plan.");

            CurrentPlanResult existingSubscription = await _currentPlanService.GetCurrentPlanAsync(_currentUser.UserId, ct);
            if (existingSubscription.IsFree == false)
                throw new ConflictException("You already have an active subscription.");

            if (existingSubscription.CurrentCoachCount > planPrice.Plan.MaxCoaches)
                throw new BadRequestException("You already have more coaches than the new plan allows.");

            if (existingSubscription.CurrentGymCount > planPrice.Plan.MaxOwnedGyms)
                throw new BadRequestException("You already have more gyms than the new plan allows.");

            if (existingSubscription.CurrentMemberCount > planPrice.Plan.MaxMembers)
                throw new BadRequestException("You already have more members than the new plan allows.");

            decimal discountAmount = 0m;
            int? couponId = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var couponResult = await _couponService.ValidateCouponAsync(
                    dto.CouponCode,
                    planPrice.Amount,
                    planPrice.PlanId,
                    ct);

                if (!couponResult.IsValid)
                    throw new BadRequestException(couponResult.Message!);

                if (await _paymentRepo.HasUsedThisCouponBeforeAsync(_currentUser.UserId, couponResult.CouponId!.Value, ct))
                    throw new ConflictException("You have already used this coupon before.");

                discountAmount = couponResult.DiscountAmount;
                couponId = couponResult.CouponId;
            }

            dto.OriginalAmount = planPrice.Amount;
            dto.DiscountAmount = discountAmount;
            dto.FinalAmount = dto.OriginalAmount - discountAmount;
            dto.CurrencyCode = planPrice.CurrencyCode;
            dto.CouponId = couponId;

            try
            {
                await _unitOfWork.BeginTransactionAsync(ct);

                if (couponId.HasValue)
                {
                    await _couponService.IncrementUsageAsync(couponId.Value, ct);
                }

                var entity = await base.AddAsync(dto, ct);
                await _publishEndpoint.Publish(new PaymentCreatedEvent(entity.Id), ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                return entity;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<PaymentRequestRDTO> ApproveAsync(int id, PaymentRequestApprove dto, CancellationToken ct = default)
        {
            var entity = await _paymentRepo.GetByIdAsync(id, true, ct);
            if (entity == null)
                throw new NotFoundException("Payment request not found.");

            if (entity.Status != PaymentRequestStatus.Pending)
                throw new BadRequestException("Only pending payment requests can be approved.");

            entity.Status = PaymentRequestStatus.Approved;
            entity.ReviewNotes = dto.ReviewNotes;
            entity.ReviewedBy = _currentUser.UserId;
            entity.ReviewedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            await _publishEndpoint.Publish(new PaymentApprovedEvent(entity.Id, entity.CreatedById, entity.CouponId, entity.DiscountAmount), ct);

            return _mapper.Map<PaymentRequestRDTO>(entity);
        }

        public async Task<PaymentRequestRDTO> RejectAsync(int id, PaymentRequestReject dto, CancellationToken ct = default)
        {
            var entity = await _paymentRepo.GetByIdAsync(id, true, ct);
            if (entity == null)
                throw new NotFoundException("Payment request not found.");

            if (entity.Status != PaymentRequestStatus.Pending)
                throw new BadRequestException("Only pending payment requests can be rejected.");

            try
            {
                await _unitOfWork.BeginTransactionAsync(ct);
                entity.Status = PaymentRequestStatus.Rejected;
                entity.RejectionReason = dto.RejectionReason;
                entity.ReviewedBy = _currentUser.UserId;
                entity.ReviewedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(ct);

                if (entity.CouponId.HasValue)
                {
                    await _couponService.DecrementUsageAsync(entity.CouponId.Value, ct);
                }

                await _unitOfWork.CommitTransactionAsync(ct);
                await _publishEndpoint.Publish(new PaymentRejectedEvent(entity.Id, entity.CreatedById, entity.RejectionReason), ct);

                return _mapper.Map<PaymentRequestRDTO>(entity);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}
```

#### `CurrentPlanService.cs`
```csharp
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using AutoMapper;
using Domain.Enum;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service.shared
{
    public class CurrentPlanService : ICurrentPlanService
    {
        private readonly IOwnerSubscriptionRepo _subscriptionRepo;
        private readonly ISubscriptionPlanRepo _planRepo;
        private readonly IGymRepo _gymRepo;
        private readonly IGymPersonRepo _gymPersonRepo;
        private readonly IMapper _mapper;

        public CurrentPlanService(
            IOwnerSubscriptionRepo subscriptionRepo,
            ISubscriptionPlanRepo planRepo,
            IGymRepo gymRepo,
            IGymPersonRepo gymPersonRepo,
            IMapper mapper)
        {
            _subscriptionRepo = subscriptionRepo;
            _planRepo = planRepo;
            _gymRepo = gymRepo;
            _gymPersonRepo = gymPersonRepo;
            _mapper = mapper;
        }

        public async Task<CurrentPlanResult> GetCurrentPlanAsync(int ownerUserId, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepo.GetCurrentSubscriptionAsync(ownerUserId, ct);
            int gymCount = await _gymRepo.CountOwnedByOwnerAsync(ownerUserId, ct);
            int memberCount = await _gymPersonRepo.CountPeopleTypeByOwnerAsync(ownerUserId, PersonType.Member, ct);
            int coachCount = await _gymPersonRepo.CountPeopleTypeByOwnerAsync(ownerUserId, PersonType.Staff);

            if (subscription != null && subscription.Status == OwnerSubscriptionStatus.Active)
            {
                return new CurrentPlanResult
                {
                    PlanId = subscription.Plan.Id,
                    PlanName = subscription.Plan.Name,
                    MaxOwnedGyms = subscription.Plan.MaxOwnedGyms,
                    MaxMembers = subscription.Plan.MaxMembers,
                    MaxCoaches = subscription.Plan.MaxCoaches,
                    IsFree = false,
                    Subscription = _mapper.Map<OwnerSubscriptionRDTO>(subscription),
                    SubscriptionStatus = subscription.Status,
                    CurrentGymCount = gymCount,
                    CurrentMemberCount = memberCount,
                    CurrentCoachCount = coachCount,
                };
            }

            var freePlan = await _planRepo.GetFreePlanAsync(ct);
            if (freePlan == null)
                throw new ApplicationException("Free subscription plan was not found.");

            return new CurrentPlanResult
            {
                PlanId = freePlan.Id,
                PlanName = freePlan.Name,
                MaxOwnedGyms = freePlan.MaxOwnedGyms,
                MaxMembers = freePlan.MaxMembers,
                MaxCoaches = freePlan.MaxCoaches,
                IsFree = true,
                Subscription = null,
                SubscriptionStatus = OwnerSubscriptionStatus.Active,
                CurrentGymCount = gymCount,
                CurrentMemberCount = memberCount,
                CurrentCoachCount = coachCount,
            };
        }
    }
}
```

---

### 2.6 Event Consumer

#### `PaymentRequestApprovedConsumer.cs`
```csharp
using System.Threading.Tasks;
using Application.Interface.Repo;
using Application.Interface.Service;
using Domain.Events;
using Domain.Model;
using MassTransit;

namespace Application.EventConsumer
{
    public class PaymentRequestApprovedConsumer : IConsumer<PaymentApprovedEvent>
    {
        private readonly IOwnerSubscriptionService _subscriptionService;
        private readonly ICouponRedemptionRepo _couponRedemptionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentRequestApprovedConsumer(
            IOwnerSubscriptionService subscriptionService,
            ICouponRedemptionRepo couponRedemptionRepo,
            IUnitOfWork unitOfWork)
        {
            _subscriptionService = subscriptionService;
            _couponRedemptionRepo = couponRedemptionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
        {
            int paymentRequestId = context.Message.PaymentRequestId;
            int userId = context.Message.UserId;
            int? couponId = context.Message.CouponId;
            decimal? discountAmount = context.Message.DiscountAmount;

            await _subscriptionService.CreateFromApprovedPaymentAsync(paymentRequestId, context.CancellationToken);

            if (couponId.HasValue && discountAmount.HasValue)
            {
                var couponRedemption = new CouponRedemption
                {
                    CouponId = couponId.Value,
                    PaymentRequestId = paymentRequestId,
                    DiscountAmount = discountAmount.Value,
                    CreatedById = userId,
                };
                await _couponRedemptionRepo.AddAsync(couponRedemption, context.CancellationToken);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            }
        }
    }
}
```

---

## 3. 🏛️ Infrastructure Layer

### 3.1 EF Core Configurations

#### `SubscriptionPlanConfig.cs`
```csharp
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.IsFree).IsRequired();
        }
    }
}
```

#### `PlanPriceConfig.cs`
```csharp
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class PlanPriceConfiguration : IEntityTypeConfiguration<PlanPrice>
    {
        public void Configure(EntityTypeBuilder<PlanPrice> builder)
        {
            builder.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Plan)
                   .WithMany(x => x.Prices)
                   .HasForeignKey(x => x.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PlanId, x.CountryCode, x.CurrencyCode, x.DurationMonths }).IsUnique();

            builder.ToTable(t => t.HasCheckConstraint("CK_PlanPrices_Duration", "\"DurationMonths\" > 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_PlanPrices_Amount", "\"Amount\" >= 0"));
        }
    }
}
```

#### `OwnerSubscriptionConfig.cs`
```csharp
using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class OwnerSubscriptionConfiguration : IEntityTypeConfiguration<OwnerSubscription>
    {
        public void Configure(EntityTypeBuilder<OwnerSubscription> builder)
        {
            builder.ConfigureAuditing();

            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
            builder.Property(x => x.AmountPaid).HasPrecision(18, 2);

            builder.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PlanPrice).WithMany().HasForeignKey(x => x.PlanPriceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PaymentRequest).WithMany(x => x.OwnerSubscriptions).HasForeignKey(x => x.PaymentRequestId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CreatedById, x.EndDate });

            builder.ToTable(t => t.HasCheckConstraint("CK_OwnerSubscriptions_Dates", "\"StartDate\" < \"EndDate\""));
        }
    }
}
```

#### `PaymentRequestConfig.cs`
```csharp
using Domain.Model;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
    {
        public void Configure(EntityTypeBuilder<PaymentRequest> builder)
        {
            builder.ConfigureFileAuditing();

            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
            builder.Property(x => x.CouponCode).HasMaxLength(50);
            builder.Property(x => x.ReviewNotes).HasMaxLength(1000);
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);

            builder.Property(x => x.OriginalAmount).HasPrecision(18, 2);
            builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            builder.Property(x => x.FinalAmount).HasPrecision(18, 2);

            builder.HasOne(x => x.ReviewedByUser)
                   .WithMany()
                   .HasForeignKey(x => x.ReviewedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PlanPrice).WithMany().HasForeignKey(x => x.PlanPriceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Coupon).WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CreatedById, x.Status });

            builder.ToTable(t => t.HasCheckConstraint("CK_PaymentRequests_Amount", "\"FinalAmount\" >= 0"));
        }
    }
}
```

---

### 3.2 Repositories

#### `SubscriptionPlanRepo.cs`
```csharp
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repo.Entity
{
    public class SubscriptionPlanRepo(ApplicationDbContext context, ILogger<SubscriptionPlanRepo> logger, QueryCache queryCache)
        : BaseRepo<SubscriptionPlan>(context, logger, queryCache), ISubscriptionPlanRepo
    {
        protected override Func<IQueryable<SubscriptionPlan>, IQueryable<SubscriptionPlan>>? Includes()
        {
            return query => query.Include(x => x.Prices);
        }

        public override Task<PaginatedRes<SubscriptionPlan>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<SubscriptionPlan>, IQueryable<SubscriptionPlan>>? include = null)
        {
            include ??= Includes();
            return base.GetPageAsync(searchReq, trackChanges, cancellationToken, include);
        }

        public override Task<SubscriptionPlan?> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            Func<IQueryable<SubscriptionPlan>, IQueryable<SubscriptionPlan>>? include = null)
        {
            include ??= Includes();
            return base.GetByIdAsync(id, trackChanges, cancellationToken, include);
        }

        public override Task<SubscriptionPlan?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return base.GetByIdAsync(id, false, cancellationToken, Includes());
        }

        public Task<PlanPrice> AddPlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default)
        {
            context.PlanPrice.Add(planPrice);
            return Task.FromResult(planPrice);
        }

        public Task<PlanPrice> DeletePlanPriceAsync(PlanPrice planPrice, CancellationToken cancellationToken = default)
        {
            context.PlanPrice.Remove(planPrice);
            return Task.FromResult(planPrice);
        }

        public async Task<PlanPrice?> GetPlanPriceByIdAsync(int id, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            IQueryable<PlanPrice> query = trackChanges ? context.PlanPrice : context.PlanPrice.AsNoTracking();

            return await query
                .Include(x => x.Plan)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<SubscriptionPlan?> GetFreePlanAsync(CancellationToken ct = default)
        {
            return await context.SubscriptionPlan
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsFree, ct);
        }
    }
}
```

#### `OwnerSubscriptionRepo.cs`
```csharp
using Application.Interface.Repo;
using Application.Model;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class OwnerSubscriptionRepo(ApplicationDbContext context, ILogger<OwnerSubscriptionRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseAuditableRepo<OwnerSubscription>(context, logger, queryCache, currentUser), IOwnerSubscriptionRepo
    {
        protected override Func<IQueryable<OwnerSubscription>, IQueryable<OwnerSubscription>>? Includes()
        {
            return query => query.Include(x => x.Plan).Include(x => x.PlanPrice).Include(x => x.PaymentRequest);
        }

        public override Task<OwnerSubscription?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return base.GetByIdAsync(id, false, cancellationToken, Includes());
        }

        public async Task<OwnerSubscription?> GetCurrentSubscriptionAsync(int ownerUserId, CancellationToken ct = default)
        {
            return await DbSet
                .Include(x => x.Plan)
                .Include(x => x.PlanPrice)
                .FirstOrDefaultAsync(x =>
                    x.CreatedById == ownerUserId &&
                    DateTime.UtcNow <= x.EndDate, ct);
        }
    }
}
```

#### `PaymentRequestRepo.cs`
```csharp
using Application.Interface.Repo;
using Application.Model;
using Domain.Enum;
using Domain.Model;
using Infrastructure.Cache;
using Infrastructure.Persistence;
using Infrastructure.Repo.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class PaymentRequestRepo(ApplicationDbContext context, ILogger<PaymentRequestRepo> logger, QueryCache queryCache, CurrentUser currentUser)
        : BaseAuditableRepo<PaymentRequest>(context, logger, queryCache, currentUser), IPaymentRequestRepo
    {
        protected override Func<IQueryable<PaymentRequest>, IQueryable<PaymentRequest>>? Includes()
        {
            return query => query.Include(x => x.Plan).Include(x => x.PlanPrice).Include(x => x.Coupon);
        }

        public override Task<PaymentRequest?> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return base.GetByIdAsync(id, false, cancellationToken, Includes());
        }

        public Task<bool> HasPendingRequestAsync(int UserId, CancellationToken ct = default)
        {
            return DbSet.AnyAsync(x =>
                x.CreatedById == UserId &&
                x.Status == PaymentRequestStatus.Pending, ct);
        }

        public async Task<bool> HasUsedThisCouponBeforeAsync(int UserId, int CouponId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(x =>
                x.CreatedById == UserId &&
                x.CouponId == CouponId &&
                x.Status == PaymentRequestStatus.Approved, ct);
        }
    }
}
```

---

## 4. 🌐 API Controllers Layer

#### `SubscriptionPlanController.cs`
```csharp
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(RoleType.SuperAdmin))]
    public class SubscriptionPlanController(ILogger<SubscriptionPlanController> logger, ISubscriptionPlanService service) : ControllerBase
    {
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanRDTO>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            PaginatedRes<SubscriptionPlanRDTO> SubscriptionPlans = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<SubscriptionPlanRDTO>>.Success(SubscriptionPlans));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SubscriptionPlanRDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var SubscriptionPlan = await service.GetByIdDetailsAsync(id, cancellationToken: cancellationToken);
            return Ok(Result<SubscriptionPlanRDTO>.Success(SubscriptionPlan));
        }

        #region Plans

        [HttpPost("Create")]
        public async Task<ActionResult<SubscriptionPlanRDTO>> CreateAsync([FromBody] SubscriptionPlanCDTO SubscriptionPlanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var createdSubscriptionPlan = await service.AddAsync(SubscriptionPlanDto);
            return Ok(Result<SubscriptionPlanRDTO>.Success(createdSubscriptionPlan));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id, [FromBody] SubscriptionPlanUDTO SubscriptionPlanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updatedSubscriptionPlan = await service.UpdateAsync(id, SubscriptionPlanDto);
            return Ok(Result<SubscriptionPlanRDTO>.Success(updatedSubscriptionPlan));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var deletedSubscriptionPlan = await service.DeleteAsync(id);
            return Ok(Result<SubscriptionPlanRDTO>.Success(deletedSubscriptionPlan));
        }

        #endregion

        #region PlanPrices

        [HttpPost("{PlanId}/PlanPrices/Create")]
        public async Task<ActionResult<PlanPriceRDTO>> CreatePlanPriceAsync(int PlanId, [FromBody] PlanPriceCDTO planPriceDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            PlanPriceRDTO createdPlanPrice = await service.AddPlanPriceAsync(PlanId, planPriceDto);
            return Ok(Result<PlanPriceRDTO>.Success(createdPlanPrice));
        }

        [HttpPut("{PlanId}/PlanPrices/{id}")]
        public async Task<ActionResult<PlanPriceRDTO>> UpdatePlanPriceAsync(int PlanId, int id, [FromBody] PlanPriceUDTO planPriceDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            PlanPriceRDTO updatedPlanPrice = await service.UpdatePlanPriceAsync(id, planPriceDto);
            return Ok(Result<PlanPriceRDTO>.Success(updatedPlanPrice));
        }

        [HttpDelete("PlanPrices/{id}")]
        public async Task<ActionResult<PlanPriceRDTO>> DeletePlanPriceAsync(int id)
        {
            PlanPriceRDTO deletedPlanPrice = await service.DeletePlanPriceAsync(id);
            return Ok(Result<PlanPriceRDTO>.Success(deletedPlanPrice));
        }

        #endregion
    }
}
```

#### `OwnerSubscriptionController.cs`
```csharp
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerSubscriptionController(
        ILogger<OwnerSubscriptionController> logger,
        IOwnerSubscriptionService service,
        ICurrentPlanService currentPlanService,
        CurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaginatedRes<OwnerSubscriptionRDTO>>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            PaginatedRes<OwnerSubscriptionRDTO> OwnerSubscriptions = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<OwnerSubscriptionRDTO>>.Success(OwnerSubscriptions));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Result<OwnerSubscriptionRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var OwnerSubscription = await service.GetByIdDetailsAsync(id, cancellationToken);
            return Ok(Result<OwnerSubscriptionRDTO>.Success(OwnerSubscription));
        }

        [HttpGet("get-my-current-subscription")]
        [Authorize]
        public async Task<ActionResult<Result<CurrentPlanResult>>> GetMySubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            CurrentPlanResult OwnerSubscriptions = await currentPlanService.GetCurrentPlanAsync(currentUser.UserId, ct: cancellationToken);
            return Ok(Result<CurrentPlanResult>.Success(OwnerSubscriptions));
        }
    }
}
```

#### `PaymentRequestController.cs`
```csharp
using Application.DTO;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Application.Interface.Service;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentRequestController(ILogger<PaymentRequestController> logger, IPaymentRequestService service) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaginatedRes<PaymentRequestRDTO>>>> GetPagedAsync([FromBody] PaginatedSearchReq searchReq)
        {
            PaginatedRes<PaymentRequestRDTO> PaymentRequests = await service.GetPageAsync(searchReq, false);
            return Ok(Result<PaginatedRes<PaymentRequestRDTO>>.Success(PaymentRequests));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Result<PaymentRequestRDTO>>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var PaymentRequest = await service.GetByIdDetailsAsync(id, cancellationToken);
            return Ok(Result<PaymentRequestRDTO>.Success(PaymentRequest));
        }

        [HttpPost("Create")]
        [Authorize]
        public async Task<ActionResult<Result<PaymentRequestRDTO>>> CreateAsync([FromForm] PaymentRequestCDTO PaymentRequestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            PaymentRequestDto.IsPublic = false;
            var createdPaymentRequest = await service.AddAsync(PaymentRequestDto);
            return Ok(Result<PaymentRequestRDTO>.Success(createdPaymentRequest));
        }

        [HttpPut("Approve/{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaymentRequestRDTO>>> ApproveAsync(int id, PaymentRequestApprove dto)
        {
            var approvedPaymentRequest = await service.ApproveAsync(id, dto);
            return Ok(Result<PaymentRequestRDTO>.Success(approvedPaymentRequest));
        }

        [HttpPut("Reject/{id}")]
        [Authorize(Roles = $"{AppRole.SuperAdmin}")]
        public async Task<ActionResult<Result<PaymentRequestRDTO>>> RejectAsync(int id, PaymentRequestReject dto)
        {
            var rejectedPaymentRequest = await service.RejectAsync(id, dto);
            return Ok(Result<PaymentRequestRDTO>.Success(rejectedPaymentRequest));
        }
    }
}
```

---

## 5. 🛠️ Configuration & DI Registration Instructions

```csharp
// 1. Repositories
services.AddScoped<ISubscriptionPlanRepo, SubscriptionPlanRepo>();
services.AddScoped<IOwnerSubscriptionRepo, OwnerSubscriptionRepo>();
services.AddScoped<IPaymentRequestRepo, PaymentRequestRepo>();

// 2. Services
services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
services.AddScoped<IOwnerSubscriptionService, OwnerSubscriptionService>();
services.AddScoped<IPaymentRequestService, PaymentRequestService>();
services.AddScoped<ICurrentPlanService, CurrentPlanService>();

// 3. DbContext DbSets
public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }
public DbSet<PlanPrice> PlanPrice { get; set; }
public DbSet<OwnerSubscription> OwnerSubscription { get; set; }
public DbSet<PaymentRequest> PaymentRequest { get; set; }
```
