// Application/Service/Base/BaseFileService.cs
using Application.Cache;
using Application.DTO.Base;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Interface.Service.Shared.Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Events;
using Domain.Model.Base;
using MassTransit;

namespace Application.Service
{
    /// <summary>
    /// أي Service بتاع Entity فيه ملف يورث من هنا بدل BaseService العادي.
    /// مسؤول عن رفع الملف على الـ Storage بعد التحقق منه (اللي بيتم في
    /// الـ DTO نفسه عن طريق [AllowedFileTypes])، وحذف الملف القديم من
    /// الـ Storage عند الـ Update أو الـ Delete.
    /// </summary>
    public abstract class BaseFileService<T, RDTO, CDTO, UDTO>
        : BaseService<T, RDTO, CDTO, UDTO>
        where T : BaseFileEntity
        where RDTO : BaseFRDTO
        where CDTO : BaseFCDTO
        where UDTO : BaseFUDTO
    {
        protected readonly IStorageService _storageService;

        protected BaseFileService(
            IBaseRepo<T> repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            IStorageService storageService)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser)
        {
            _storageService = storageService;
        }

        public override async Task<RDTO> AddAsync(CDTO dto, CancellationToken cancellationToken = default)
        {
            string storedFileName = await _storageService.UploadFileToStorageAsync(dto.File, cancellationToken);
            string fileUrl = _storageService.GenerateUrlToAccessFileAsync(storedFileName, cancellationToken);

            T entity = _mapper.Map<T>(dto);
            entity.FileUrl = fileUrl;
            entity.StoredFileName = storedFileName;

            T added = await _repo.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), added.Id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(added);
        }

        public override async Task<RDTO> UpdateAsync(int id, UDTO dto, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);
            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            if (dto.File is not null)
            {
                string oldStoredFileName = entity.StoredFileName;

                string newStoredFileName = await _storageService.UploadFileToStorageAsync(dto.File, cancellationToken);
                string newFileUrl = _storageService.GenerateUrlToAccessFileAsync(newStoredFileName, cancellationToken);

                _mapper.Map(dto, entity);
                entity.FileUrl = newFileUrl;
                entity.StoredFileName = newStoredFileName;

                // نمسح القديم بعد ما الجديد اترفع بنجاح
                if (!string.IsNullOrWhiteSpace(oldStoredFileName))
                    await _storageService.DeleteFileFromStorageAsync(oldStoredFileName, cancellationToken);
            }
            else
            {
                _mapper.Map(dto, entity);
            }

            T updated = await _repo.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            return _mapper.Map<RDTO>(updated);
        }

        public override async Task<RDTO> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken: cancellationToken);
            if (entity is null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            await _repo.DeleteAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(entity.StoredFileName))
                await _storageService.DeleteFileFromStorageAsync(entity.StoredFileName, cancellationToken);

            return _mapper.Map<RDTO>(entity);
        }
    }
}