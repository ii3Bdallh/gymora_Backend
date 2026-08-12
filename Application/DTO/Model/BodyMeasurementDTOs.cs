using System;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record BodyMeasurementCDTO : BaseAuditableCDTO
    {
        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 500, ErrorMessage = "Weight must be positive and reasonable")]
        public decimal WeightKg { get; set; }

        [Range(0, 300, ErrorMessage = "Height must be positive and reasonable")]
        public decimal? HeightCm { get; set; }

        [Range(0, 100, ErrorMessage = "Body fat percentage must be between 0 and 100")]
        public decimal? BodyFatPercentage { get; set; }

        [Range(0, 300, ErrorMessage = "Chest measurement must be positive")]
        public decimal? ChestCm { get; set; }

        [Range(0, 300, ErrorMessage = "Waist measurement must be positive")]
        public decimal? WaistCm { get; set; }

        [Range(0, 150, ErrorMessage = "Arms measurement must be positive")]
        public decimal? ArmsCm { get; set; }

        [Range(0, 200, ErrorMessage = "Legs measurement must be positive")]
        public decimal? LegsCm { get; set; }

        [MaxLength(500, ErrorMessage = "Notes must not exceed 500 characters")]
        public string? Notes { get; set; }
    }

    public record BodyMeasurementUDTO : BaseAuditableUDTO
    {
        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 500, ErrorMessage = "Weight must be positive and reasonable")]
        public decimal WeightKg { get; set; }

        [Range(0, 300, ErrorMessage = "Height must be positive and reasonable")]
        public decimal? HeightCm { get; set; }

        [Range(0, 100, ErrorMessage = "Body fat percentage must be between 0 and 100")]
        public decimal? BodyFatPercentage { get; set; }

        [Range(0, 300, ErrorMessage = "Chest measurement must be positive")]
        public decimal? ChestCm { get; set; }

        [Range(0, 300, ErrorMessage = "Waist measurement must be positive")]
        public decimal? WaistCm { get; set; }

        [Range(0, 150, ErrorMessage = "Arms measurement must be positive")]
        public decimal? ArmsCm { get; set; }

        [Range(0, 200, ErrorMessage = "Legs measurement must be positive")]
        public decimal? LegsCm { get; set; }

        [MaxLength(500, ErrorMessage = "Notes must not exceed 500 characters")]
        public string? Notes { get; set; }
    }

    public record BodyMeasurementRDTO : BaseAuditableRDTO
    {
        public decimal WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? ChestCm { get; set; }
        public decimal? WaistCm { get; set; }
        public decimal? ArmsCm { get; set; }
        public decimal? LegsCm { get; set; }
        public string? Notes { get; set; }
    }
}
