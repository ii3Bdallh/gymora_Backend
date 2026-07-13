
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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
        // public int? CouponId { get; set; }
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
    }
}