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
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service
{
    public class ExpenseService : BaseGymService<Expense, ExpenseRDTO, ExpenseCDTO, ExpenseUDTO>, IExpenseService
    {
        private readonly IGymPersonRepo _gymPersonRepo;

        public ExpenseService(
            IExpenseRepo repo,
            IGymPersonRepo gymPersonRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            CurrentUser currentUser,
            ILogger<ExpenseService> logger)
            : base(repo, unitOfWork, mapper, cacheService, publishEndpoint, currentUser, logger)
        {
            _gymPersonRepo = gymPersonRepo;
        }

        protected override async Task BeforeAddAsync(ExpenseCDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeAddAsync(dto, cancellationToken);
            await ValidateExpensePayloadAsync(dto.GymStaffId, cancellationToken);
        }

        protected override async Task BeforeUpdateAsync(Expense entity, ExpenseUDTO dto, CancellationToken cancellationToken)
        {
            await base.BeforeUpdateAsync(entity, dto, cancellationToken);
            await ValidateExpensePayloadAsync(dto.GymStaffId, cancellationToken);
        }

        private async Task ValidateExpensePayloadAsync(int? staffId, CancellationToken ct)
        {
            // Validate staff if provided
            if (staffId.HasValue)
            {
                var staff = await _gymPersonRepo.GetByIdAsync(staffId.Value, false, ct);
                if (staff == null)
                    throw new NotFoundException($"Gym staff with ID {staffId.Value} was not found.");

                if (staff.GymId != (CurrentGymId ?? 0))
                    throw new InvalidOperationException("The specified staff member does not belong to this gym.");
            }
        }
    }
}
