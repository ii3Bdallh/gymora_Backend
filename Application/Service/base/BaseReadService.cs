using Application.Cache;
using Application.DTO;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Interface;
using Domain.Model.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    /// <summary>
    /// Implements read-only operations with ownership-based filtering support.
    /// Public entities are visible to everyone, while owned entities are filtered by CreatedById.
    /// </summary>
    public abstract class BaseReadService<T, RDTO> : IBaseReadService<T, RDTO>
        where T : class, IBaseEntity
        where RDTO : BaseRDTO
    {
        protected readonly IBaseRepo<T> _repo;
        protected readonly IMapper _mapper;
        protected readonly ICacheService _cacheService;
        protected readonly CurrentUser _currentUser;
        protected readonly ILogger _logger;

        protected BaseReadService(
            IBaseRepo<T> repo,
            IMapper mapper,
            ICacheService cacheService,
            CurrentUser currentUser,
            ILogger logger)
        {
            _repo = repo;
            _mapper = mapper;
            _cacheService = cacheService;
            _currentUser = currentUser;
            _logger = logger;
        }

        protected int? CurrentGymId => _currentUser.CurrentGymId;
        protected int CurrentUserId => _currentUser.UserId;

        protected int? CurrentPersonId => _currentUser.CurrentPersonId;
        protected CurrentUser CurrentUser => _currentUser;
        protected virtual bool IsCacheEnabled =>
             typeof(ICacheableEntity).IsAssignableFrom(typeof(T));


        protected bool HasFullAccess => CurrentUser.IsSuperAdmin;

        // protected virtual bool CanAccess(T entity)
        // {
        //     return HasFullAccess || entity.CreatedById == CurrentUserId;
        // }


        /// <summary>
        /// بيحدد هل نبعت userId للـ CacheKeyGenerator ولا لأ.
        /// بنبعته بس لو الـ Entity Owned واليوزر مش SuperAdmin，
        /// لأن دول هما الحالة الوحيدة اللي محتاجة عزل الكاش لكل يوزر لوحده.
        /// </summary>
        protected virtual int? CacheUserScope =>
            typeof(IOnlyMeCanSee).IsAssignableFrom(typeof(T)) && !HasFullAccess
                ? CurrentUserId
                : (typeof(IOnlyMeCanSeeAtGym).IsAssignableFrom(typeof(T)) && !HasFullAccess && !CurrentUser.IsGymOwner && !CurrentUser.IsGymManager)
                    ? CurrentPersonId
                    : null;


        protected async Task<TResult> GetOrCreateCacheAsync<TResult>(
            string key,
            Func<Task<TResult>> factory,
            bool enableCache = true)
        {
            if (!enableCache)
                return await factory();

            var cached = await _cacheService.GetAsync<TResult>(key);

            if (cached is not null)
            {
                _logger.LogInformation("Cache hit: {Key}", key);
                return cached;
            }

            var result = await factory();

            await _cacheService.SetAsync(key, result);

            return result;
        }


        public virtual async Task<IEnumerable<RDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all {EntityType} records", typeof(T).Name);



            var models = await _repo.GetAllAsync(cancellationToken: cancellationToken); // استخدم await إذا كانت الطريقة GetAllAsync تدعم ذلك

            var result = _mapper.Map<IEnumerable<RDTO>>(models);

            _logger.LogInformation("Fetched {Count} {EntityType} records", models.Count(), typeof(T).Name);
            return result;
        }


        #region Read By Id
        public virtual async Task<RDTO> GetByIdAsync(
            int id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var key = CacheKeyGenerator.ById<T>(
                id,
                CurrentGymId,
                CacheUserScope);

            return await GetOrCreateCacheAsync(
                key,
                async () =>
                {
                    _logger.LogInformation(
                        "Fetching {EntityType} with ID {Id}",
                        typeof(T).Name,
                        id);

                    var entity = await _repo.GetByIdAsync(
                         id,
                         trackChanges,
                         cancellationToken);

                    if (entity is null)
                        throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

                    var dto = _mapper.Map<RDTO>(entity);

                    await AfterMapReadAsync(entity, dto, cancellationToken);

                    return dto;
                },
                IsCacheEnabled);
        }

        public virtual async Task<RDTO> GetByIdDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var key = CacheKeyGenerator.ById<T>(
                id,
                CurrentGymId,
                CacheUserScope) + ":details";

            return await GetOrCreateCacheAsync(
                key,
                async () =>
                {
                    _logger.LogInformation(
                        "Fetching detailed {EntityType} with ID {Id}",
                        typeof(T).Name,
                        id);

                    var entity = await _repo.GetByIdDetailsAsync(
                         id,
                         cancellationToken);

                    if (entity is null)
                        throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

                    var dto = _mapper.Map<RDTO>(entity);

                    await AfterMapReadAsync(entity, dto, cancellationToken);

                    return dto;
                },
                IsCacheEnabled);
        }

        protected virtual Task AfterMapReadAsync(
            T entity,
            RDTO dto,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        public virtual async Task<PaginatedRes<RDTO>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var page = await _repo.GetPageAsync(
                searchReq,
                trackChanges,
                cancellationToken);

            return new PaginatedRes<RDTO>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
                Items = _mapper.Map<List<RDTO>>(page.Items)
            };
        }

       
    }


}
