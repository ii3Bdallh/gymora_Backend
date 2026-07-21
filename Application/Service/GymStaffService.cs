using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Service.Base;


using Application.DTO.Model;
using Application.Service.Shared;
using Application.Interface.Service.Shared;
using MassTransit;
using Application.Model;

namespace Application.Service
{
    public class GymStaffService : BaseService<GymStaff, GymStaffRDTO, GymStaffCDTO, GymStaffUDTO>, IGymStaffService
    {
        private readonly IGymStaffRepo _gymStaffRepo;
        public GymStaffService(
            IGymStaffRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<GymStaffService> logger
            )
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymStaffRepo = repo;
        }

        public Task<GymStaffRDTO> GetByGymIdAndUserIdAsync(int gymId, int userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<GymStaffRDTO> LinkAccountToGymAsync(int gymId, Guid inviteCode, CancellationToken ct = default)
        {

            var gymStaff = await _gymStaffRepo.LinkAccountToGymAsync(gymId, inviteCode, ct);

            if (gymStaff is null)
                throw new InvalidOperationException("Failed to link account to gym.");
              await _unitOfWork.SaveChangesAsync(ct);
            var result = _mapper.Map<GymStaffRDTO>(gymStaff);

            return result;
        }
    }
}