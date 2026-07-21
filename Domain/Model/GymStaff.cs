using Domain.Attributes;
using Domain.Enum;
using Domain.Model.Auth;
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
  public class GymStaff : BaseGymEntity
  {

    public int? UserId { get; set; } // FK -> ApplicationUser, nullable: unregistered staff

    public ApplicationUser? User { get; set; } // Navigation property to ApplicationUser

    [Searchable]
    public string StaffName { get; set; } = null!;

    public Guid StaffInviteCode { get; set; } = Guid.NewGuid();

    public string? PhoneNumber { get; set; }

    [Searchable]
    public string? Email { get; set; }

    public GymRole GymRole { get; set; }

    [Filterable(FilterType.Between)]
    public decimal? Salary { get; set; }

    public DateTime? SalaryEffectiveFrom { get; set; }

    [Filterable(FilterType.Between)]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;


    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;


  }


}