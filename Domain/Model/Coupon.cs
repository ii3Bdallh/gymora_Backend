using Domain.Attributes;
using Domain.Enum;
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
  public class Coupon : BaseEntity
  {
    [Searchable]
    public string Code { get; set; } = null!;

    [Searchable]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Filterable(FilterType.Exact)]
    public DiscountType DiscountType { get; set; }

    [Filterable(FilterType.Between)]
    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }

    [Filterable(FilterType.Between)]
    public DateTime ValidFrom { get; set; }

    [Filterable(FilterType.Between)]
    public DateTime ValidTo { get; set; }

    public bool IsFirstPurchaseOnly { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    // public ICollection<CouponPlan> CouponPlans { get; set; } = new List<CouponPlan>();
  }


}