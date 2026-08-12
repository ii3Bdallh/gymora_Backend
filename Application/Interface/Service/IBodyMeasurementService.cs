using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface IBodyMeasurementService : IBaseService<BodyMeasurement, BodyMeasurementRDTO, BodyMeasurementCDTO, BodyMeasurementUDTO>
    {
    }
}
