using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly TestRepository _repository;
    private readonly IEmailService _emailService;

    private readonly INotificationService _notificationService;

    public TestController(TestRepository repository, IEmailService emailService, INotificationService notificationService)
    {
        _repository = repository;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Test()
    {
        await _repository.TestEvent(
            1,
            "Hello From Event");

        return Ok();
    }

    [HttpPost("send-notification")]
    [AllowAnonymous]
    public async Task<IActionResult> TestSendNotification()
    {
        string result = await _notificationService.SendNotificationTestAsync(
             new Application.DTO.NotificationDTO
             {
                 Title = "Test Notification",
                 Body = "Hello From Notification Service"
             });

        return Ok(result);
    }

    [HttpPost("send-email")]
    [AllowAnonymous]
    public async Task<IActionResult> TestSendEmail()
    {
        await _emailService.SendEmailTestAsync(
            "AbdallhMamdouh079@gmail.com",
            "Test Email",
            "Hello From Email Service"
        );

        return Ok();
    }
}
