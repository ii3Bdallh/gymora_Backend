using Application.DTO;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.DTO.Pagintion;
using Application.Interface.Repo;
using Application.Interface.Service;
using AutoMapper;
using Domain.Model.Base;

namespace Application.Service
{
    public abstract class BaseService<T, RDTO, CDTO, UDTO> : IBaseService<T, RDTO, CDTO, UDTO>
     where T : BaseEntity
     where RDTO : BaseRDTO
     where CDTO : BaseCDTO
     where UDTO : BaseUDTO
    {
        protected readonly IBaseRepo<T> _repo;
        protected readonly IUnitOfWork _unitOfWork; // 👈 إضافة الـ UnitOfWork هنا
        protected readonly IMapper _mapper;

        protected BaseService(IBaseRepo<T> repo, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;a
            _mapper = mapper;
        }

        public virtual async Task<IEnumerable<RDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var models = await _repo.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<RDTO>>(models);
        }

        public virtual async Task<PaginatedRes<RDTO>> GetPageAsync(
            PaginatedSearchReq searchReq,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            var page = await _repo.GetPageAsync(searchReq, isActive, trackChanges, cancellationToken);

            return new PaginatedRes<RDTO>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
                Items = _mapper.Map<IEnumerable<RDTO>>(page.Items)
            };
        }

        public virtual async Task<RDTO> GetByIdAsync(
            int id,
            bool isActive = true,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive, trackChanges, cancellationToken);

            // 👈 التحقق هنا مكانه الصحيح هندسياً
            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            return _mapper.Map<RDTO>(entity);
        }

        public virtual async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            T entity = _mapper.Map<T>(dto);

            T added = await _repo.AddAsync(entity, cancellationToken);

            // 👈 حفظ التغييرات بعد استجابة الـ Repo في الـ Memory
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<RDTO>(added);
        }

        public virtual async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            // بنجيبه بـ trackChanges = true عشان الـ EF Core يلاحظ التعديلات علطول
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            _mapper.Map(dto, entity);

            T updated = await _repo.UpdateAsync(entity, cancellationToken);

            // 👈 حفظ التعديل مركزياً
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<RDTO>(updated);
        }

        public virtual async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);


            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            await _repo.DeleteAsync(entity, cancellationToken);


            // 👈 حفظ الـ Soft Delete
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<RDTO>(entity);
        }
    }
}