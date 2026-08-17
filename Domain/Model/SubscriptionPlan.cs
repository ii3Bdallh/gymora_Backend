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
  public class SubscriptionPlan : BaseEntity, ICacheableEntity
  {
    [Searchable]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

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


