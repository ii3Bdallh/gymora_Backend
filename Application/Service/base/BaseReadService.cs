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
        where T : BaseEntity
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
        protected CurrentUser CurrentUser => _currentUser;

        /// <summary>
        /// بيحدد هل نبعت userId للـ CacheKeyGenerator ولا لأ.
        /// بنبعته بس لو الـ Entity Owned واليوزر مش SuperAdmin،
        /// لأن دول هما الحالة الوحيدة اللي محتاجة عزل الكاش لكل يوزر لوحده.
        /// </summary>
        private int? CacheUserScope =>
            typeof(IOwnedEntity).IsAssignableFrom(typeof(T)) && !_currentUser.IsSuperAdmin
                ? _currentUser.UserId
                : (int?)null;


        public virtual async Task<IEnumerable<RDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all {EntityType} records", typeof(T).Name);



            var models = await _repo.GetAllAsync(cancellationToken: cancellationToken); // استخدم await إذا كانت الطريقة GetAllAsync تدعم ذلك

            var result = _mapper.Map<IEnumerable<RDTO>>(models);

            _logger.LogInformation("Fetched {Count} {EntityType} records", models.Count(), typeof(T).Name);
            return result;
        }

        public virtual async Task<RDTO> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var entityName = CacheEntityNames.ForType<T>();
            var key = CacheKeyGenerator.ById(entityName, id, CurrentGymId, CacheUserScope);

            var cached = await _cacheService.GetAsync<RDTO>(key);
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit for {EntityType} with ID {Id}", typeof(T).Name, id);
                return cached;
            }

            _logger.LogInformation("Fetching {EntityType} with ID {Id}", typeof(T).Name, id);





            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: false, cancellationToken: cancellationToken);   // أفضل من FindAsync مع الفلتر

            if (entity is null)
            {
                _logger.LogWarning("{EntityType} with ID {Id} was not found", typeof(T).Name, id);
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");
            }

            // Additional ownership check for GetById (defense in depth)
            if (entity is IOwnedEntity owned && !_currentUser.IsSuperAdmin && owned.CreatedById != _currentUser.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to access {EntityType} ID {Id} which he does not own",
                    _currentUser.UserId, typeof(T).Name, id);
                throw new UnauthorizedAccessException("You do not have permission to access this resource.");
            }

            var dto = _mapper.Map<RDTO>(entity);
            await _cacheService.SetAsync(key, dto);

            return dto;
        }

        public virtual async Task<PaginatedRes<RDTO>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var entityName = CacheEntityNames.ForType<T>();
            var key = CacheKeyGenerator.ByPage(entityName, searchReq, CurrentGymId, CacheUserScope);

            var cached = await _cacheService.GetAsync<PaginatedRes<RDTO>>(key);
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit for {EntityType} page {PageNumber}", typeof(T).Name, searchReq.PageNumber);
                return cached;
            }

            _logger.LogInformation("Fetching page {PageNumber} of {EntityType}", searchReq.PageNumber, typeof(T).Name);




            var page = await _repo.GetPageAsync(searchReq, isActive, trackChanges, cancellationToken, Includes());
            // إذا كان GetPageAsync في الـ Repo يدعم IQueryable، يفضل تعديله لاحقًا

            var dtoPage = new PaginatedRes<RDTO>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
                Items = _mapper.Map<List<RDTO>>(page.Items)
            };

            await _cacheService.SetAsync(key, dtoPage);
            _logger.LogInformation("Fetched page {PageNumber}/{TotalPages} of {EntityType} ({TotalCount} total)",
                page.PageNumber, (int)Math.Ceiling((double)page.TotalCount / page.PageSize), typeof(T).Name, page.TotalCount);

            return dtoPage;
        }

        protected virtual Func<IQueryable<T>, IQueryable<T>>? Includes()
        {
            return null;
        }
    }
}