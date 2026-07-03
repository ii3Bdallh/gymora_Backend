using Application.DTO.Base;
using Application.DTO.Base.Auditable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Read
{
    public record TopicRDTO : BaseRDTO
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int SubjectId { get; init; }

    }
}
