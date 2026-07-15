using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
  // Dont Forget to add [Searchable] , [Filterable] , 
  // Dont Forget to add Config , UDTO , CDTO , RDTO
  public class PaymentRequest : BaseAuditableFileEntity, IOwnedEntity
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

    public int? ReviewedBy { get; set; } // FK added - missing in original SQL

    [Filterable(FilterType.Between)]
    public DateTime? ReviewedAt { get; set; }


    // Navigational Properties

    public int PlanId { get; set; }
    public SubscriptionPlan Plan { get; set; } = null!;

    public int PlanPriceId { get; set; }
    public PlanPrice PlanPrice { get; set; } = null!;



    public int? CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

  }
}