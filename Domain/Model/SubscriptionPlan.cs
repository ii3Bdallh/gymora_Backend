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
  public class SubscriptionPlan : BaseEntity
  {
    [Searchable]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    [Filterable(FilterType.Between)]
    public int MaxOwnedGyms { get; set; }

    [Filterable(FilterType.Between)]
    public int MaxCoachesPerGym { get; set; }

    [Filterable(FilterType.Between)]
    public int MaxMembersPerGym { get; set; }

    public string? FeaturesJson { get; set; }

    [Filterable(FilterType.Between)]
    public DateTime CreatedOn { get; set; }

    public ICollection<PlanPrice> Prices { get; set; } = new List<PlanPrice>();
  }

}


