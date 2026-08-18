using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Domain.Enum;

namespace Application.DTO.Model
{
    public record ExerciseCDTO : BaseAuditableFCDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters")]
        public string Name { get; set; } = null!;

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Primary muscle is required")]
        public MuscleGroup PrimaryMuscle { get; set; }

        public MuscleGroup? SecondaryMuscle { get; set; }

        public ExerciseEquipment? Equipment { get; set; }

        [Required(ErrorMessage = "Difficulty level is required")]
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

        [MaxLength(1000, ErrorMessage = "Video URL must not exceed 1000 characters")]
        public string? VideoUrl { get; set; }
    }

    public record ExerciseUDTO : BaseAuditableFUDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters")]
        public string Name { get; set; } = null!;

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Primary muscle is required")]
        public MuscleGroup PrimaryMuscle { get; set; }

        public MuscleGroup? SecondaryMuscle { get; set; }

        public ExerciseEquipment? Equipment { get; set; }

        [Required(ErrorMessage = "Difficulty level is required")]
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

        [MaxLength(1000, ErrorMessage = "Video URL must not exceed 1000 characters")]
        public string? VideoUrl { get; set; }
    }

    public record ExerciseRDTO : BaseAuditableFRDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public MuscleGroup PrimaryMuscle { get; set; }
        public MuscleGroup? SecondaryMuscle { get; set; }
        public ExerciseEquipment? Equipment { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public string? VideoUrl { get; set; }
        public bool IsApproved { get; set; }
    }
}
