using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Model;
using Application.Service.Base;
using AutoMapper;
using Domain.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    public class UserWorkoutBlockService : BaseService<UserWorkoutBlock, UserWorkoutBlockRDTO, UserWorkoutBlockCDTO, UserWorkoutBlockUDTO>, IUserWorkoutBlockService
    {
        public UserWorkoutBlockService(
            IUserWorkoutBlockRepo repo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<UserWorkoutBlockService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
        }

        private void EnforceSuperAdmin()
        {
            if (!CurrentUser.IsSuperAdmin)
            {
                throw new ForbiddenException("Only SuperAdmins are allowed to manage user workout blocks.");
            }
        }

        protected override Task BeforeAddAsync(UserWorkoutBlockCDTO dto, CancellationToken cancellationToken)
        {
            EnforceSuperAdmin();
            return base.BeforeAddAsync(dto, cancellationToken);
        }

        protected override Task BeforeUpdateAsync(UserWorkoutBlock entity, UserWorkoutBlockUDTO dto, CancellationToken cancellationToken)
        {
            EnforceSuperAdmin();
            return base.BeforeUpdateAsync(entity, dto, cancellationToken);
        }

        protected override Task BeforeDeleteAsync(UserWorkoutBlock entity, CancellationToken cancellationToken)
        {
            EnforceSuperAdmin();
            return base.BeforeDeleteAsync(entity, cancellationToken);
        }

        public async Task UnblockUserAsync(int userId, CancellationToken cancellationToken)
        {
            EnforceSuperAdmin();

            var activeBlocks = await _repo.DbSet
                .Where(x => x.BlockedUserId == userId && x.BlockedUntil > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            if (activeBlocks.Any())
            {
                foreach (var block in activeBlocks)
                {
                    await _repo.DeleteAsync(block, cancellationToken);
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
