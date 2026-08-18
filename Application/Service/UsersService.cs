using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service;
using Application.Service.Base;
using Application.DTO.Model;
using Application.DTO.Exceptions;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;
using Domain.Model.Auth;
using Application.DTO.Pagintion;
using Gymora.Contracts.Authentication;
using Microsoft.AspNetCore.Http;

namespace Application.Service
{
    public class UsersService(
        IUsersRepo repo,
        IUserRepo userRepo,
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        IMapper mapper,
        ILogger<UsersService> logger
    ) : IUsersService
    {

        public async Task<IEnumerable<ApplicationUserRDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching all {EntityType} records", nameof(ApplicationUser));

            var models = await repo.GetAllAsync(cancellationToken: cancellationToken);

            var result = mapper.Map<IEnumerable<ApplicationUserRDTO>>(models);

            logger.LogInformation("Fetched {Count} {EntityType} records", models.Count(), nameof(ApplicationUser));
            return result;
        }

        public async Task<ApplicationUserRDTO> GetByIdAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching {EntityType} with ID {Id}", nameof(ApplicationUser), id);

            var entity = await repo.GetByIdAsync(
                id,
                isActive,
                trackChanges,
                cancellationToken);

            if (entity is null)
                throw new NotFoundException($"User with ID {id} was not found.");


            var dto = mapper.Map<ApplicationUserRDTO>(entity);


            return dto;
        }

        public async Task<ApplicationUserRDTO> GetByIdDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching {EntityType} details with ID {Id}", nameof(ApplicationUser), id);

            var entity = await repo.GetByIdDetailsAsync(id, cancellationToken);

            if (entity is null)
                throw new NotFoundException($"{nameof(ApplicationUser)} with ID {id} was not found.");

            return mapper.Map<ApplicationUserRDTO>(entity);
        }

        public async Task<PaginatedRes<ApplicationUserRDTO>> GetPageAsync(PaginatedSearchReq searchReq, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            var page = await repo.GetPageAsync(
                searchReq,
                isActive,
                trackChanges,
                cancellationToken);

            return new PaginatedRes<ApplicationUserRDTO>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
                Items = mapper.Map<List<ApplicationUserRDTO>>(page.Items)
            };
        }

        public async Task<UserProfileRDTO> GetUserProfileAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await userRepo.GetByIdAsync(userId, false, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User profile not found.");
            }

            var roles = await userRepo.GetUserRolesAsync(userId, cancellationToken);
            var platformRole = roles.FirstOrDefault() ?? "User";

            return new UserProfileRDTO(
                UserId: user.Id.ToString(),
                PersonName: user.PersonName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                PhoneNumber: user.PhoneNumber,
                ProfilePictureUrl: user.ProfileImageUrl,
                CreatedAt: user.CreatedOn,
                PlatformRole: platformRole
            );
        }

        public async Task<UserProfileRDTO> UpdateUserProfileAsync(int userId, UserProfileUDTO updateDto, CancellationToken cancellationToken = default)
        {
            var user = await userRepo.GetByIdAsync(userId, true, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User profile not found.");
            }

            if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber) && user.PhoneNumber != updateDto.PhoneNumber)
            {
                bool isPhoneInUse = await userRepo.IsPhoneNumberUsedByOtherUserAsync(updateDto.PhoneNumber, userId, cancellationToken);
                if (isPhoneInUse)
                {
                    throw new ConflictException("This phone number is already registered to another account.");
                }
                user.PhoneNumber = updateDto.PhoneNumber;
            }

            user.PersonName = updateDto.PersonName;
            
            if (!string.IsNullOrWhiteSpace(updateDto.ProfilePictureUrl))
            {
                user.ProfileImageUrl = updateDto.ProfilePictureUrl;
            }

            await userRepo.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var roles = await userRepo.GetUserRolesAsync(userId, cancellationToken);
            var platformRole = roles.FirstOrDefault() ?? "User";

            return new UserProfileRDTO(
                UserId: user.Id.ToString(),
                PersonName: user.PersonName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                PhoneNumber: user.PhoneNumber,
                ProfilePictureUrl: user.ProfileImageUrl,
                CreatedAt: user.CreatedOn,
                PlatformRole: platformRole
            );
        }

        public async Task<UserProfileRDTO> UploadProfilePictureAsync(int userId, IFormFile file, CancellationToken cancellationToken = default)
        {
            var user = await userRepo.GetByIdAsync(userId, true, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User profile not found.");
            }

            string? oldStoredFilePath = user.ProfileImageUrl;

            string storedFilePath = await storageService.UploadFileToStorageAsync(
                file,
                isPublic: true,
                entityType: "Users",
                cancellationToken: cancellationToken);

            string fileUrl = storageService.GetFileAccessUrl(storedFilePath, isPublic: true);

            user.ProfileImageUrl = fileUrl;

            if (!string.IsNullOrWhiteSpace(oldStoredFilePath))
            {
                await storageService.DeleteFileFromStorageAsync(oldStoredFilePath, cancellationToken);
            }

            await userRepo.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var roles = await userRepo.GetUserRolesAsync(userId, cancellationToken);
            var platformRole = roles.FirstOrDefault() ?? "User";

            return new UserProfileRDTO(
                UserId: user.Id.ToString(),
                PersonName: user.PersonName ?? string.Empty,
                Email: user.Email ?? string.Empty,
                PhoneNumber: user.PhoneNumber,
                ProfilePictureUrl: user.ProfileImageUrl,
                CreatedAt: user.CreatedOn,
                PlatformRole: platformRole
            );
        }
    }
}