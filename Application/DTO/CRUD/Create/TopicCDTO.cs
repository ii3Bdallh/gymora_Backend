using Application.DTO.Base;
using Application.DTO.Base.Auditable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Create
{
    public record TopicCDTO : BaseCDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; init; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; init; }


        [Required(ErrorMessage = "SubjectId is required")]
        public int SubjectId { get; init; }

        
    }
}
