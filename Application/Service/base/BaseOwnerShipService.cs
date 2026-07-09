// Application/Service/Base/BaseAuditableService.cs
using Application.DTO;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using AutoMapper;
using Domain.Model.Base;
using MassTransit;
using Application.Cache;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.DTO.Base.Auditable;
using Domain.Events;

namespace Application.Service.Base
{
    /// <summary>
    /// Base service for entities that are auditable and visible to users within the same gym.
    /// مثال: Gym, Member, WorkoutPlan, Exercise, Revenue, etc.
    /// </summary>
    public abstract class BaseOwnerShipService<T, RDTO, CDTO, UDTO>
        : BaseAuditableService<T, RDTO, CDTO, UDTO>
        where T : AuditableEntity
        where RDTO : BaseAuditableRDTO
        where CDTO : BaseAuditableCDTO
        where UDTO : BaseAuditableUDTO
    {
        protected BaseOwnerShipService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser)
        {
        }

        // Read Operations: متاحة للكل داخل الجيم
        public override async Task<RDTO> GetByIdAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken ct = default)
        {
            var dto = await base.GetByIdAsync(id, isActive, trackChanges, ct);

            if (!CanRead(dto))
                throw new ForbiddenException("You do not have access to this private resource.");

            return dto;
        }

        public override async Task<PaginatedRes<RDTO>> GetPageAsync(PaginatedSearchReq searchReq, bool isActive = true, bool trackChanges = false, CancellationToken ct = default)
        {
            throw new NotImplementedException("GetPageAsync is not implemented in BaseOwnerShipService. You need to override it in the derived service.");
        }
        protected virtual bool CanRead(BaseAuditableRDTO entity)
        {
            if (CurrentUser.IsSuperAdmin)
                return true;

            // Owner or Admin in the gym
            // if (CurrentUser.IsInGymRole("Owner") || CurrentUser.IsInGymRole("Admin"))
            //     return true;

            // Owner of the record
            if (entity.CreatedById == CurrentUserId)
                return true;

            return false;
        }
    }
}