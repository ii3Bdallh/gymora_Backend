using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record SessionCDTO : BaseAuditableCDTO
    {
        [Required(ErrorMessage = "Session name is required")]
        [MaxLength(100, ErrorMessage = "Session name must not exceed 100 characters")]
        public string SessionName { get; set; } = null!;

        public ICollection<SessionExerciseCDTO> Exercises { get; set; } = new List<SessionExerciseCDTO>();
    }

    public record SessionUDTO : BaseAuditableUDTO
    {
        [Required(ErrorMessage = "Session name is required")]
        [MaxLength(100, ErrorMessage = "Session name must not exceed 100 characters")]
        public string SessionName { get; set; } = null!;
    }

    public record SessionRDTO : BaseAuditableRDTO
    {
        public string SessionName { get; set; } = null!;
        public bool IsApproved { get; set; }
        public ICollection<SessionExerciseRDTO> Exercises { get; set; } = new List<SessionExerciseRDTO>();
    }
}
