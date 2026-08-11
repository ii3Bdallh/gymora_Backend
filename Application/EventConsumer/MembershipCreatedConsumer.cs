using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.EventConsumer
{
    public class MembershipCreatedConsumer
  : IConsumer<MembershipCreatedEvent>
    {
        private readonly ILogger<MembershipCreatedConsumer> _logger;

        public MembershipCreatedConsumer(ILogger<MembershipCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<MembershipCreatedEvent> context)
        {
            throw new NotImplementedException("To Do : Membership Created Event Consumed: GymPersonId = {GymPersonId}, MembershipId = {MembershipId}, GymId = {GymId}");
        }

    }
}