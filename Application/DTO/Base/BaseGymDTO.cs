
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Application.DTO.Base
{
    public record BaseGymCDTO : BaseCDTO
    {
        [BindNever]
        public int GymId { get; set; }
    }
    public record BaseGymUDTO : BaseUDTO
    {
        [BindNever]
        public int GymId { get; set; }
    }
    public record BaseGymRDTO : BaseRDTO
    {
        public int GymId { get; set; }

    }
}
