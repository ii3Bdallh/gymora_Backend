using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
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
  public class CouponRedemption : BaseAuditableEntity, IOnlyMeCanSee //  , IPublicEntity
  {
    [Filterable(FilterType.Exact)]
    public int CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;


    [Filterable(FilterType.Exact)]
    public int PaymentRequestId { get; set; }
    public PaymentRequest PaymentRequest { get; set; } = null!;

    [Filterable(FilterType.Between)]
    public decimal DiscountAmount { get; set; }

  }
}