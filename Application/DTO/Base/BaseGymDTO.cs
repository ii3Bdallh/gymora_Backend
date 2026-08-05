
namespace Application.DTO.Base
{
    public record BaseGymCDTO : BaseCDTO
    {
        public required int GymId { get; init; }
    }
    public record BaseGymUDTO : BaseUDTO
    {
        public required int GymId { get; init; }
    }
    public record BaseGymRDTO : BaseRDTO
    {
        public int GymId { get; init; }

    }
}
