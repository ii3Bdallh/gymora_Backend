using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Interface;
using Domain.Model.Base;

namespace Domain.Model
{
    public class BodyMeasurement : BaseAuditableEntity, IOnlyMeCanSee
    {


        public decimal WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? ChestCm { get; set; }
        public decimal? WaistCm { get; set; }
        public decimal? ArmsCm { get; set; }
        public decimal? LegsCm { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

    }
}
