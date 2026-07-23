using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Base;
using Domain.Model.Base.Domain.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
  // Dont Forget to add [Searchable] , [Filterable] , 
  // Dont Forget to add Config , UDTO , CDTO , RDTO
  public class Gym : BaseAuditableFileEntity // , IOnlyMeCanSee // , IPublicEntity
  {
    [Searchable]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }

    [Filterable(FilterType.Between)]
    public decimal Latitude { get; set; }
    [Filterable(FilterType.Between)]
    public decimal Longitude { get; set; }


    [Filterable(FilterType.Exact)]
    public GymStatus Status { get; set; } = GymStatus.Active;

    public byte[]? RowVersion { get; set; }       // ROWVERSION

    // Navigation Properties


  }
}