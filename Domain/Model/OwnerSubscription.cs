using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
  // Dont Forget to add [Searchable] , [Filterable] , 
  // Dont Forget to add Config , UDTO , CDTO , RDTO
  public class OwnerSubscription : BaseAuditableEntity // , IOwnedEntity , IPublicEntity
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

    [Filterable(FilterType.Between)]
    public DateTime GraceEndDate { get; set; }

    [NotMapped]
    public OwnerSubscriptionStatus Status
    {
      get
      {
        var now = DateTime.UtcNow;

        if (now <= EndDate)
          return OwnerSubscriptionStatus.Active;

        if (now <= GraceEndDate)
          return OwnerSubscriptionStatus.Grace;

        return OwnerSubscriptionStatus.Expired;
      }
    }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;


  }
}