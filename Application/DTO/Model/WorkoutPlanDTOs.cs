using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;

namespace Application.DTO.Model
{
    public record WorkoutPlanCDTO : BaseAuditableFCDTO
    {
        [Required(ErrorMessage = "Plan name is required")]
        [MaxLength(200, ErrorMessage = "Plan name must not exceed 200 characters")]
        public string PlanName { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sessions are required")]
        [MinLength(1, ErrorMessage = "Workout plan must have at least one session")]
        public ICollection<SessionCDTO> Sessions { get; set; } = new List<SessionCDTO>();
    }

    public record WorkoutPlanUDTO : BaseAuditableFUDTO
    {
        [Required(ErrorMessage = "Plan name is required")]
        [MaxLength(200, ErrorMessage = "Plan name must not exceed 200 characters")]
        public string PlanName { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string? Description { get; set; }
    }

    public record WorkoutPlanRDTO : BaseAuditableFRDTO
    {
        public string PlanName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<SessionRDTO> Sessions { get; set; } = new List<SessionRDTO>();
    }
}
