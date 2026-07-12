using Application.DTO.TrainerCertificate;

namespace Application.Interface.Service;

public interface ITrainerCertificateService
    : IBaseService<Domain.Model.TrainerCertificate, TrainerCertificateRDTO, TrainerCertificateCDTO, TrainerCertificateUDTO>
{
}
