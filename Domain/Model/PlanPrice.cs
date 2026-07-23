using Domain.Attributes;
using Domain.Enum;
using Domain.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
  // Dont Forget to add [Searchable] , [Filterable] , 
  // Dont Forget to add Config , UDTO , CDTO , RDTO
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

