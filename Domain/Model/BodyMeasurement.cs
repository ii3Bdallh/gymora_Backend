using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Base;

namespace Domain.Model
{
    public class BodyMeasurement : BaseAuditableEntity, IOnlyMeCanSee
    {
        [Filterable(FilterType.Between)]
        public decimal WeightKg { get; set; }

        [Filterable(FilterType.Between)]
        public decimal? HeightCm { get; set; }

        [Filterable(FilterType.Between)]
        public decimal? BodyFatPercentage { get; set; }

        [Filterable(FilterType.Between)]
        public decimal? ChestCm { get; set; }

        [Filterable(FilterType.Between)]
        public decimal? WaistCm { get; set; }

        [Filterable(FilterType.Between)]
        public decimal? ArmsCm { get; set; }

        [Filterable(FilterType.Between)]
        public decimal? LegsCm { get; set; }

        [Searchable]
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
