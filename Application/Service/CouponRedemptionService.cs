using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Service.Base;


using Application.DTO.Model;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;

namespace Application.Service
{
        public class CouponRedemptionService : BaseAuditableService<CouponRedemption, CouponRedemptionRDTO, CouponRedemptionCDTO, CouponRedemptionUDTO>, ICouponRedemptionService
    {
        public CouponRedemptionService(
            ICouponRedemptionRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<CouponRedemptionService> logger
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
        }
    }
}