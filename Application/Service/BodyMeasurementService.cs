using System;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    public class BodyMeasurementService : BaseAuditableService<BodyMeasurement, BodyMeasurementRDTO, BodyMeasurementCDTO, BodyMeasurementUDTO>, IBodyMeasurementService
    {
        public BodyMeasurementService(
            IBodyMeasurementRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<BodyMeasurementService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
        }
    }
}
