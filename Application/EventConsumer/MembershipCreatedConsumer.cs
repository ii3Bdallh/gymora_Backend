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
    public class MembershipCreatedConsumer : IConsumer<MembershipCreatedEvent>
    {
        private readonly IRevenueService _revenueService;
        private readonly CurrentUser _currentUser;
        private readonly ILogger<MembershipCreatedConsumer> _logger;

        public MembershipCreatedConsumer(
            IRevenueService revenueService,
            CurrentUser currentUser,
            ILogger<MembershipCreatedConsumer> logger)
        {
            _revenueService = revenueService;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<MembershipCreatedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Membership Created Event Consumed: GymPersonId = {GymPersonId}, Amount = {Amount}, GymId = {GymId}",
                msg.GymPersonId, msg.FinalAmount, msg.GymId);

            // Set user scope to bypass base service / repo restriction
            _currentUser.CurrentGymId = msg.GymId;
            _currentUser.PlatformRole = AppRole.SuperAdmin;
            _currentUser.CurrentPersonId = msg.CreatedByPersonId;

            var dto = new RevenueCDTO
            {
                GymId = msg.GymId,
                RevenueCategory = RevenueCategory.Membership,
                GymMemberId = msg.GymPersonId,
                Amount = msg.FinalAmount,
                PaymentMethod = PaymentMethod.Cash,
                Description = $"Auto-created from membership plan: {msg.PlanName}",
                RevenueDate = msg.MembershipStartDate
            };

            await _revenueService.AddAsync(dto, context.CancellationToken);

            _logger.LogInformation("Successfully auto-created Revenue record for Member GymPersonId: {GymPersonId}", msg.GymPersonId);
        }
    }
}