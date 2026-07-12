using Application.DTO.TrainerCertificate;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Model;
using MassTransit;

namespace Application.Service;

public class TrainerCertificateService
    : BaseAuditableFileService<TrainerCertificate, TrainerCertificateRDTO, TrainerCertificateCDTO, TrainerCertificateUDTO>,
      ITrainerCertificateService
{
    public TrainerCertificateService(
        ITrainerCertificateRepo repo,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService,
        IPublishEndpoint publishEndpoint,
        CurrentUser currentUser,
        IStorageService storageService)
        : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, storageService)
    {
    }
}
