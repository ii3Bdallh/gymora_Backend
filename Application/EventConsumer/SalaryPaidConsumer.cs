using System;
using System.Threading.Tasks;
using Domain.Events;
using Domain.Enum;
using Application.DTO.Model;
using Application.Interface.Service;
using Application.Model;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.EventConsumer
{
    public class SalaryPaidConsumer : IConsumer<SalaryPaidEvent>
    {
        private readonly IExpenseService _expenseService;
        private readonly CurrentUser _currentUser;
        private readonly ILogger<SalaryPaidConsumer> _logger;

        public SalaryPaidConsumer(
            IExpenseService expenseService,
            CurrentUser currentUser,
            ILogger<SalaryPaidConsumer> logger)
        {
            _expenseService = expenseService;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SalaryPaidEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Salary Paid Event Consumed: GymPersonId = {GymPersonId}, Amount = {Amount}, GymId = {GymId}",
                msg.GymPersonId, msg.Amount, msg.GymId);

            // Set user scope to bypass base service / repo restriction
            _currentUser.CurrentGymId = msg.GymId;
            _currentUser.PlatformRole = AppRole.SuperAdmin;
            _currentUser.CurrentPersonId = msg.PaidByPersonId;

            var dto = new ExpenseCDTO
            {
                GymId = msg.GymId,
                ExpenseCategory = ExpenseCategory.Salary,
                GymStaffId = msg.GymPersonId,
                Amount = msg.Amount,
                PaymentMethod = PaymentMethod.Cash,
                Description = $"Auto-created from salary payment for period: {msg.PeriodFrom:yyyy-MM-dd} to {msg.PeriodTo:yyyy-MM-dd}",
                ExpenseDate = msg.PaidAt
            };

            await _expenseService.AddAsync(dto, context.CancellationToken);

            _logger.LogInformation("Successfully auto-created Expense record for Staff GymPersonId: {GymPersonId}", msg.GymPersonId);
        }
    }
}
