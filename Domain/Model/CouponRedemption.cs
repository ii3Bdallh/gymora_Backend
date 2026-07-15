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
  public class CouponRedemption : BaseAuditableEntity  , IOwnedEntity //  , IPublicEntity
  {
    public int CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;


    public int PaymentRequestId { get; set; }
    public PaymentRequest PaymentRequest { get; set; } = null!;

    [Filterable(FilterType.Between)]
    public decimal DiscountAmount { get; set; }

  }
}