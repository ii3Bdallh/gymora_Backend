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
using Application.Interface.Service;
using Application.Service.Base;
using Application.DTO.Model;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;
using Domain.Model.Auth;
using Application.DTO.Pagintion;

namespace Application.Service
{
    public class UsersService(
        IUsersRepo repo,
        IMapper mapper,
        ILogger<UsersService> logger
    ) : IUsersService
    { // <-- تم إضافة قوس البداية المفقود هنا

        // تم إضافة async هنا لتتوافق مع await بالداخل
        public async Task<IEnumerable<UsersRDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // تم استبدال typeof(T).Name بـ nameof(ApplicationUser)
            logger.LogInformation("Fetching all {EntityType} records", nameof(ApplicationUser));

            var models = await repo.GetAllAsync(cancellationToken: cancellationToken);

            // تم تغيير RDTO إلى UsersRDTO
            var result = mapper.Map<IEnumerable<UsersRDTO>>(models);

            logger.LogInformation("Fetched {Count} {EntityType} records", models.Count(), nameof(ApplicationUser));
            return result;
        }

        // تم إضافة async وتعديل أسماء المتغيرات لتطابق الـ Constructor
        public async Task<UsersRDTO> GetByIdAsync(int id, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Fetching {EntityType} with ID {Id}", nameof(ApplicationUser), id);

            var entity = await repo.GetByIdAsync(
                id,
                isActive,
                trackChanges,
                cancellationToken);

            if (entity is null)
                throw new Exception($"{nameof(ApplicationUser)} with ID {id} was not found."); // استبدلها بـ NotFoundException الخاصة بك إن وجدت


            var dto = mapper.Map<UsersRDTO>(entity);


            return dto;
        }

        // تم إضافة async هنا أيضاً وتعديل الأسماء
        public async Task<PaginatedRes<UsersRDTO>> GetPageAsync(PaginatedSearchReq searchReq, bool isActive = true, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            var page = await repo.GetPageAsync(
                searchReq,
                isActive,
                trackChanges,
                cancellationToken);

            return new PaginatedRes<UsersRDTO>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
                Items = mapper.Map<List<UsersRDTO>>(page.Items)
            };
        }


    }
}