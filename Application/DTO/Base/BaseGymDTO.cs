
namespace Application.DTO.Base
{
    public record BaseGymCDTO : BaseCDTO
    {

    }
    public record BaseGymUDTO : BaseUDTO
    {

    }
    public record BaseGymRDTO : BaseRDTO
    {
        public int GymId { get; init; }

    }
}
