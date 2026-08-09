using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Model;
using MassTransit;
using Microsoft.Extensions.Logging;

using Application.Service.Base;

namespace Application.Service
{
    public class MembershipPlanService : BaseAuditableGymService<MembershipPlan, MembershipPlanRDTO, MembershipPlanCDTO, MembershipPlanUDTO>, IMembershipPlanService
    {
        public MembershipPlanService(
            IMembershipPlanRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<MembershipPlanService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
        }
    }
}
