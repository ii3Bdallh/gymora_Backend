using System.Threading.Tasks;
using Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.EventConsumer
{
    public class SalaryPaidConsumer : IConsumer<SalaryPaidEvent>
    {
        private readonly ILogger<SalaryPaidConsumer> _logger;

        public SalaryPaidConsumer(ILogger<SalaryPaidConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SalaryPaidEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Salary Paid Event Consumed: StaffId = {StaffId}, Amount = {Amount}, PaidAt = {PaidAt}, GymId = {GymId}",
                msg.StaffId, msg.Amount, msg.PaidAt, msg.GymId);
            return Task.CompletedTask;
        }
    }
}
