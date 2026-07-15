using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.StaticTexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        [HttpGet("Email")]

        public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
        {
            await emailService.SendEmailAsync(
                "Abdallhmamdouh079@gmail.com",
                "Test Email",
                "This is a test email.");
            return Ok();
        }

        [HttpGet("Notification")]
        public async Task<IActionResult> TestNotification([FromServices] INotificationService notificationService)
        {
            var Notification = new NotificationDTO
            {
                Title = "test Notification",
                Body = "This is a test notification.",
            };
            await notificationService.SendToTopicAsync(
                NotificationTopic.AdminTopic,

Notification
                    );
            return Ok();
        }

        [HttpGet("NotificationEvent")]
        public async Task<IActionResult> TestNotificationEvent([FromServices] INotificationService notificationService, [FromServices] MassTransit.IPublishEndpoint publishEndpoint)
        {
            var Notification = new NotificationDTO
            {
                Title = "test Notification",
                Body = "This is a test notification.",
            };
            await publishEndpoint.Publish(new Domain.Events.TestNotificationEvent("This is a test notification event."));

            return Ok();
        }

        [HttpGet("CreateOwnerSubscription")]
        public async Task<IActionResult> TestCreateOwnerSubscription([FromServices] MassTransit.IPublishEndpoint publishEndpoint)
        {
            var Notification = new NotificationDTO
            {
                Title = "test Notification",
                Body = "This is a test notification.",
            };
            await publishEndpoint.Publish(new Domain.Events.PaymentApprovedEvent(10, 2)); // Replace 1 with the actual payment request ID

            return Ok();
        }

    }
}