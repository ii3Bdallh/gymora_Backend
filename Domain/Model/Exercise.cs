using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Enum;
using Domain.Model.Base;

namespace Domain.Model
{
    public class Exercise : BaseAuditableFileEntity
    {


        [Searchable]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Searchable]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Filterable(FilterType.Exact)]
        public MuscleGroup PrimaryMuscle { get; set; }

        [Filterable(FilterType.Exact)]
        public MuscleGroup? SecondaryMuscle { get; set; }

        [Filterable(FilterType.Exact)]
        public ExerciseEquipment? Equipment { get; set; }

        [Filterable(FilterType.Exact)]
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

        [MaxLength(1000)]
        public string? VideoUrl { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
