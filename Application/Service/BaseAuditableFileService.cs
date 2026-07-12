// Application/Service/Base/BaseAuditableFileService.cs
using Application.Cache;
using Application.DTO.Base.Auditable;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Service.Shared;
using Application.Model;
using AutoMapper;
using Domain.Events;
using Domain.Model.Base;
using MassTransit;

namespace Application.Service.Base
{
    /// <summary>
    /// نفس فكرة BaseAuditableService، بس للـ Entities اللي فيها ملف.
    /// بيرفع الملف على الـ Storage بعد التحقق منه في الـ DTO (عن طريق
    /// [AllowedFileTypes])، وبيحافظ على منطق الـ CreatedById/CanModify
    /// الموروث من BaseAuditableService.
    /// </summary>
    public abstract class BaseAuditableFileService<T, RDTO, CDTO, UDTO>
        : BaseAuditableService<T, RDTO, CDTO, UDTO>
        where T : BaseAuditableFileEntity
        where RDTO : BaseAuditableFRDTO
        where CDTO : BaseAuditableFCDTO
        where UDTO : BaseAuditableFUDTO
    {
        protected readonly IStorageService _storageService;

        protected BaseAuditableFileService(
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
            dto.CreatedById = CurrentUserId;

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
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);

            if (entity is null || !CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            dto.CreatedById = entity.CreatedById; // Preserve the original CreatedById

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
            T? entity = await _repo.GetByIdAsync(id, isActive: true, trackChanges: true, cancellationToken);

            if (entity is null || !CanModify(entity))
                throw new NotFoundException($"{typeof(T).Name} with ID {id} was not found.");

            T deleted = await _repo.DeleteAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(
                new EntityChangedEvent(CacheEntityNames.ForType<T>(), id, CurrentGymId),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(deleted.StoredFileName))
                await _storageService.DeleteFileFromStorageAsync(deleted.StoredFileName, cancellationToken);

            return _mapper.Map<RDTO>(deleted);
        }
    }
}