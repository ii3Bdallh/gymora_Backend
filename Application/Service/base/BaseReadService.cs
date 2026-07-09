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
using Domain.Model.Base;

namespace Application.Service
{
    /// <summary>
    /// Implements read-only operations. BaseService below inherits from
    /// this and adds write operations on top, so writing is never
    /// possible without the read plumbing (repo, mapper, cache, user)
    /// already being in place.
    /// </summary>
    public abstract class BaseReadService<T, RDTO> : IBaseReadService<T, RDTO>
        where T : BaseEntity
        where RDTO : BaseRDTO
    {
        protected readonly IBaseRepo<T> _repo;
        protected readonly IMapper _mapper;
        protected readonly ICacheService _cacheService;
        protected readonly CurrentUser _currentUser;

        protected BaseReadService(
            IBaseRepo<T> repo,
            IMapper mapper,
            ICacheService cacheService,
            CurrentUser currentUser)
        {
            _repo = repo;
            _mapper = mapper;
            _cacheService = cacheService;
            _currentUser = currentUser;
        }

        protected int? CurrentGymId => _currentUser.CurrentGymId;
        protected int CurrentUserId => _currentUser.UserId;
        protected CurrentUser CurrentUser => _currentUser;

        public virtual async Task<IEnumerable<RDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var models = await _repo.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<RDTO>>(models);
        }

        public virtual async Task<RDTO> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var entityName = CacheEntityNames.ForType<T>();
            var key = CacheKeyGenerator.ById(entityName, id, CurrentGymId);

            var cached = await _cacheService.GetAsync<RDTO>(key);
            if (cached is not null) return cached;

            T? entity = await _repo.GetByIdAsync(id, isActive, trackChanges, cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

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
            var key = CacheKeyGenerator.ByPage(entityName, searchReq, CurrentGymId);

            var cached = await _cacheService.GetAsync<PaginatedRes<RDTO>>(key);
            if (cached is not null) return cached;

            var page = await _repo.GetPageAsync(searchReq, isActive, trackChanges, cancellationToken);

            var dtoPage = new PaginatedRes<RDTO>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
                Items = _mapper.Map<List<RDTO>>(page.Items)
            };

            await _cacheService.SetAsync(key, dtoPage);
            return dtoPage;
        }
    }
}