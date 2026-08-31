# 💳 PaymentRequest Module Documentation (وثيقة موديول طلبت الدفع)

هذا الملف يحتوى على استخراج شامل وكامل لكل ما يتعلق بـ **PaymentRequest** داخل المشروع (Domain Models, Enums, DTOs, Repository, Service, Controller, EF Core Configuration, MassTransit Events & Consumers, Business Rules).

---

## 📋 1. Business Logic & Rules (قواعد وأدوار العمل)

### 🔄 دورة حياة طلب الدفع (Lifecycle)
1. **الإنشاء (Create / Pending)**:
   - يقوم المستثمر/المالك (`GymOwner`) برفع طلب دفع مع صورة إثبات التحويل (`File` - JPG/PNG بحجم أقصى 5MB).
   - لا يمكن للمستخدم إنشاء أكثر من طلب دفع معلق واحد في نفس الوقت (`HasPendingRequestAsync`).
   - لا يمكن إنشاء طلب دفع على خطة مجانية (Free Plan).
   - لا يمكن إنشاء طلب دفع جديد إذا كان لدى المستخدم اشتراك حالي نشط (حتى ينتهي اشتراكه الحالي).
   - **التحقق من حدود الخطة الجديدة**: يتم التأكد من أن عدد المدربين، الصالات، والأعضاء الحاليين لدى المستخدم لا يتجاوز الحد الأقصى المسموح به في الخطة الجديدة المراد الاشتراك فيها.
   - **التحقق من الكوبون (Coupon Validation)**:
     - يتم التأكد من صحة الكوبون ومطابقته لشرط عدم الاستخدام المسبق لنفس الكوبون من نفس المستخدم.
     - يتم احتساب قيمة الخصم (`DiscountAmount`) والمبلغ النهائي (`FinalAmount`).
     - يتم زيادة عداد استخدام الكوبون فوراً عند إنشاء الطلب (`IncrementUsageAsync`).
   - عند نجاح العملية داخل Transaction يتم نشر حدث `PaymentCreatedEvent`.

2. **القبول (Approve)**:
   - مقتصر فقط على الـ **SuperAdmin**.
   - يتم تغيير الحالة إلى `Approved` وتسجيل بيانات المراجع (`ReviewedBy`, `ReviewedAt`, `ReviewNotes`).
   - يتم نشر حدث `PaymentApprovedEvent`.
   - يقوم `PaymentRequestApprovedConsumer` بإنشاء اشتراك جديد تلقائياً (`OwnerSubscription`) وتسجيل استهلاك الكوبون (`CouponRedemption`).

3. **الرفض (Reject)**:
   - مقتصر فقط على الـ **SuperAdmin**.
   - يلزم تقديم سبب الرفض (`RejectionReason`).
   - يتم تغيير الحالة إلى `Rejected` وتسجيل بيانات المراجع.
   - **استرجاع الكوبون**: يتم إنقاص عداد استخدام الكوبون (`DecrementUsageAsync`) لإعادة توفيره.
   - يتم نشر حدث `PaymentRejectedEvent`.

---

## 🏛️ 2. Domain Layer (طبقة النطاق)

### 📄 Entity: `PaymentRequest.cs`
- **المسار**: [`Domain/Model/PaymentRequest.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Model/PaymentRequest.cs)
- **الوراثة**: `BaseAuditableFileEntity`, `IOnlyMeCanSee`

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

        // Navigation Properties
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

### 🏷️ Enum: `PaymentRequestStatus.cs`
- **المسار**: [`Domain/Enum/PaymentRequestStatus.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Enum/PaymentRequestStatus.cs)

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

### 📢 Domain Events: `PaymentEvent.cs`
- **المسار**: [`Domain/Events/PaymentEvent.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Domain/Events/PaymentEvent.cs)

```csharp
namespace Domain.Events;

public record PaymentCreatedEvent(int PaymentRequestId);

public record PaymentApprovedEvent(int PaymentRequestId, int UserId, int? CouponId, decimal? DiscountAmount);

public record PaymentRejectedEvent(int PaymentRequestId, int UserId, string RejectionReason);
```

---

## 📦 3. Data Transfer Objects (DTOs)

- **المسار**: [`Application/DTO/Model/PaymentRequestDTO.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/DTO/Model/PaymentRequestDTO.cs)

```csharp
using System.ComponentModel.DataAnnotations;
using Application.Common.FileValidation;
using Application.DTO.Base;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Application.DTO.Model
{
    // DTO الخاص بإنشاء طلب الدفع (يستقبل ملف وصورة إثبات التحويل)
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

    // DTO الخاص بالتحديث General
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

    // DTO القبول
    public record PaymentRequestApprove
    {
        public string? ReviewNotes { get; set; }
    }

    // DTO الرفض
    public record PaymentRequestReject
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        public string RejectionReason { get; set; } = null!;
    }

    // Response DTO
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

## 🗄️ 4. Persistence & Repository Layer

### 🛠️ Entity Framework Configuration: `PaymentRequestConfig.cs`
- **المسار**: [`Infrastructure/Configurations/PaymentRequestConfig.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Configurations/PaymentRequestConfig.cs)

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

### 🔹 Interface: `IPaymentRequestRepo.cs`
- **المسار**: [`Application/Interface/Repo/IPaymentRequestRepo.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Interface/Repo/IPaymentRequestRepo.cs)

```csharp
using Domain.Model;
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

### 🔹 Implementation: `PaymentRequestRepo.cs`
- **المسار**: [`Infrastructure/Repo/PaymentRequestRepo.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Infrastructure/Repo/PaymentRequestRepo.cs)

```csharp
using Application.Interface.Repo;
using Domain.Model;
using Microsoft.Extensions.Logging;
using Infrastructure.Repo.Base;
using Infrastructure.Persistence;
using Infrastructure.Cache;
using Application.Model;
using Microsoft.EntityFrameworkCore;
using Domain.Enum;

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
            return DbSet.AnyAsync(x => x.CreatedById == UserId && x.Status == PaymentRequestStatus.Pending, ct);
        }

        public async Task<bool> HasUsedThisCouponBeforeAsync(int UserId, int CouponId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(x => x.CreatedById == UserId && x.CouponId == CouponId && x.Status == PaymentRequestStatus.Approved, ct);
        }
    }
}
```

---

## ⚙️ 5. Application & Service Layer

### 🔹 Interface: `IPaymentRequestService.cs`
- **المسار**: [`Application/Interface/Service/IPaymentRequestService.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Interface/Service/IPaymentRequestService.cs)

```csharp
using Application.DTO.Model;
using Domain.Model;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IPaymentRequestService : IBaseService<PaymentRequest, PaymentRequestRDTO, PaymentRequestCDTO, PaymentRequestUDTO>
    {
        public Task<PaymentRequestRDTO> ApproveAsync(int id, PaymentRequestApprove dto, CancellationToken ct = default);
        public Task<PaymentRequestRDTO> RejectAsync(int id, PaymentRequestReject dto, CancellationToken ct = default);
    }
}
```

### 🔹 Implementation: `PaymentRequestService.cs`
- **المسار**: [`Application/Service/PaymentRequestService.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/Service/PaymentRequestService.cs)

---

## 🌐 6. API Controller Endpoints

- **المسار**: [`Api/Controllers/PaymentRequestController.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Api/Controllers/PaymentRequestController.cs)

| Method | Endpoint | Authorization | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/PaymentRequest` | `SuperAdmin` | جلب قائمة طلبات الدفع بنظام Pagination والفلترة |
| `GET` | `/api/PaymentRequest/{id}` | `Authorize` (أي مستخدم مسجل) | جلب تفاصيل طلب دفع معين بـ ID |
| `POST` | `/api/PaymentRequest/Create` | `Authorize` (GymOwner) | إنشاء طلب دفع جديد مع رفع صوره الإثبات |
| `PUT` | `/api/PaymentRequest/Approve/{id}` | `SuperAdmin` | قبول طلب الدفع المعلق وترقيه الاشتراك |
| `PUT` | `/api/PaymentRequest/Reject/{id}` | `SuperAdmin` | رفض طلب الدفع المعلق مع إبداء السبب |

---

## ⚡ 7. MassTransit Event Consumers

### 1️⃣ `PaymentRequestApprovedConsumer.cs`
- **المسار**: [`Application/EventConsumer/PaymentRequestApprovedConsumer.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/EventConsumer/PaymentRequestApprovedConsumer.cs)
- **الوظيفة**: يستمع للحدث `PaymentApprovedEvent` ليقوم بتنفيذ:
  1. إنشاء اشتراك مالك جديد (`OwnerSubscription`) عبر `_subscriptionService.CreateFromApprovedPaymentAsync`.
  2. إنشاء سجل استبدال الكوبون (`CouponRedemption`) في حال تم استخدام كوبون في الطلب.

### 2️⃣ `NotificationConsumer.cs` & `EmailConsumer.cs`
- **المسار**: [`Application/EventConsumer/NotificationConsumer.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/EventConsumer/NotificationConsumer.cs) و [`EmailConsumer.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/Application/EventConsumer/EmailConsumer.cs)
- **الوظيفة**: يستمع لأحداث `PaymentCreatedEvent` و `PaymentRejectedEvent` لإرسال إشعارات لحظية بريد إلكتروني وإشعارات داخل التطبيق للمستخدم والـ Admin.

---

## 🧪 8. Unit & Integration Tests Coverage

يتم تغطية الموديول بالاختبارات الأتية:
- **Unit Tests for Services**: [`UnitTests/Services/PaymentRequestServiceTests.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/UnitTests/Services/PaymentRequestServiceTests.cs) (تغطي إنشاء الطلب، القبول، الرفض، والتحقق من الاستثناءات وإلغاء/استرجاع الكوبونات).
- **Unit Tests for Controllers**: [`UnitTests/Controllers/PaymentRequestControllerTests.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/UnitTests/Controllers/PaymentRequestControllerTests.cs).
- **Integration Tests for Repositories**: [`IntegrationTests/Repositories/PaymentRequestRepoTests.cs`](file:///d:/Abdallah/Projects/Gymora/gymora_Backend/IntegrationTests/Repositories/PaymentRequestRepoTests.cs).
- **Postman Collection**: `Postman/03 - PaymentRequest.json`.
