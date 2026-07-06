# Event-Driven Architecture — Gymora Backend

## Architecture Overview

```
┌──────────────┐     ┌──────────────────┐     ┌─────────────────┐     ┌──────────────────────┐
│  Controller   │────▶│   Repository     │────▶│  EF Core Save   │────▶│  OutboxInterceptor   │
│  (raises      │     │  (adds domain    │     │  ChangesAsync   │     │  (captures events    │
│   event)      │     │   events)        │     │                 │     │   → OutboxMessages)  │
└──────────────┘     └──────────────────┘     └─────────────────┘     └──────────┬───────────┘
                                                                                 │
                                                                                 ▼
┌──────────────┐     ┌──────────────────┐     ┌─────────────────┐     ┌──────────────────────┐
│  Handlers     │◀────│  MediatR         │◀────│  EventDispatcher│◀────│  Hangfire OutboxWorker│
│  (Email,      │     │  IPublisher      │     │  (Deserializes  │     │  (polls unprocessed  │
│   Notification)│     │                  │     │   → publishes)  │     │   messages)          │
└──────────────┘     └──────────────────┘     └─────────────────┘     └──────────────────────┘
```

---

## 1. Domain Layer

### 1.1 Event Entity Base — `Domain/Model/Base/EventEntity.cs`

```csharp
using MediatR;

namespace Domain.Model.Base;

public abstract class EventEntity
{
    private readonly List<INotification> _domainEvents = [];

    public IReadOnlyCollection<INotification> DomainEvents
        => _domainEvents;

    public void AddDomainEvent(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

### 1.2 Concrete Event — `Domain/Events/TestEvent.cs`

```csharp
using MediatR;

namespace Domain.Events;

public sealed record TestEvent(
    int UserId,
    string Email,
    string Message) : INotification;
```

### 1.3 Event Channels — `Domain/Constant/EventChannels.cs`

```csharp
public static class EventChannels
{
    public const string Test = "test.v1";
}
```

### 1.4 Test Entity (raises domain events) — `Domain/Model/TestEntity.cs`

```csharp
using Domain.Model.Base;

namespace Domain.Model;

public class TestEntity : EventEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = default!;
}
```

### 1.5 Outbox Message Model — `Domain/Model/OutboxMessage.cs`

```csharp
namespace Domain.Model;

public class OutboxMessage
{
    public int Id { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 1.6 Notification Model — `Domain/Model/Notification.cs`

```csharp
namespace Domain.Model;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? GymId { get; set; }
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string? DataJson { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 2. Infrastructure Layer

### 2.1 Event Dispatcher Interface — `Infrastructure/Dispatching/IEventDispatcher.cs`

```csharp
namespace Infrastructure.Dispatching;

public interface IEventDispatcher
{
    Task DispatchAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken);
}
```

### 2.2 MediatR Event Dispatcher — `Infrastructure/Dispatching/MediatREventDispatcher.cs`

```csharp
using System.Text.Json;
using Domain.Events;
using MediatR;

namespace Infrastructure.Dispatching;

public class MediatREventDispatcher : IEventDispatcher
{
    private readonly IPublisher _publisher;

    public MediatREventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(
        string eventType,
        string payload,
        CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case EventChannels.Test:
                var testEvent =
                    JsonSerializer.Deserialize<TestEvent>(payload)!;
                await _publisher.Publish(testEvent, cancellationToken);
                break;
        }
    }
}
```

### 2.3 Outbox Interceptor (EF Core) — `Infrastructure/Persistence/Interceptors/OutboxInterceptor.cs`

```csharp
using System.Text.Json;
using Domain.Model;
using Domain.Model.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entities = context.ChangeTracker
            .Entries<EventEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.Entity.DomainEvents)
            {
                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    EventType = domainEvent.GetType().FullName!,
                    Payload = JsonSerializer.Serialize(
                        domainEvent,
                        domainEvent.GetType()),
                    OccurredAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            entity.Entity.ClearDomainEvents();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

### 2.4 Outbox Worker (Hangfire) — `Infrastructure/Hangfire/OutboxWorker.cs`

```csharp
using Domain.Model;
using Infrastructure.Dispatching;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Hangfire;

public class OutboxWorker
{
    private readonly ApplicationDbContext _context;
    private readonly IEventDispatcher _dispatcher;

    public OutboxWorker(
        ApplicationDbContext context,
        IEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public async Task ProcessAsync()
    {
        var messages = await _context.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        foreach (var message in messages)
        {
            try
            {
                await _dispatcher.DispatchAsync(
                    message.EventType,
                    message.Payload,
                    CancellationToken.None);

                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.ErrorMessage = ex.Message;
            }
        }

        await _context.SaveChangesAsync();
    }
}
```

### 2.5 Recurring Jobs Registration — `Infrastructure/Hangfire/RecurringJobs.cs`

```csharp
using Hangfire;

namespace Infrastructure.Hangfire;

public static class RecurringJobs
{
    public static void Register()
    {
        RecurringJob.AddOrUpdate<OutboxWorker>(
            "process-outbox-messages",
            job => job.ProcessOutboxMessagesAsync(),
            Cron.Minutely);

        RecurringJob.AddOrUpdate<TokenCleanupJob>(
            "cleanup-expired-tokens",
            job => job.CleanupExpiredTokensAsync(),
            Cron.Daily(3, 0));
    }
}
```

> **⚠️ Note:** There is a method name mismatch — `OutboxWorker` has `ProcessAsync()` but `RecurringJobs` references `ProcessOutboxMessagesAsync()`. One of these needs to be renamed.

### 2.6 Test Repository (raises events) — `Infrastructure/TestRepository.cs`

```csharp
using Domain.Events;
using Domain.Model;
using Infrastructure.Persistence;

namespace Infrastructure;

public class TestRepository
{
    private readonly ApplicationDbContext _context;

    public TestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task TestEvent(int userId, string message)
    {
        var test = new TestEntity
        {
            UserId = userId,
            Message = message
        };

        test.AddDomainEvent(
            new TestEvent(userId, "Abdallhmamdouh079@gmail.com", message));

        await _context.Tests.AddAsync(test);
        await _context.SaveChangesAsync();
    }
}
```

### 2.7 DbContext — `Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
using Domain.Model;
using Domain.Model.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Domain.Model.Notification> Notifications => Set<Domain.Model.Notification>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<TestEntity> Tests => Set<TestEntity>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
```

### 2.8 EF Configurations

#### `Infrastructure/Configurations/OutboxMessageConfiguration.cs`

```csharp
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired(false);
        builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ErrorMessage).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
    }
}
```

#### `Infrastructure/Configurations/TestEntityConfiguration.cs`

```csharp
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TestEntityConfiguration : IEntityTypeConfiguration<TestEntity>
{
    public void Configure(EntityTypeBuilder<TestEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Message).IsRequired().HasMaxLength(1000);
    }
}
```

### 2.9 Hangfire Setup — `Infrastructure/DependencyInjection/HangfireServiceCollectionExtensions.cs`

```csharp
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class HangfireServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(15)
                });
        });

        services.AddHangfireServer();
        return services;
    }
}
```

### 2.10 DI Registration — `Infrastructure/DependencyInjection/InfrastructureDependencyInjection.cs` (excerpt)

```csharp
services.AddSingleton<OutboxInterceptor>();                              // line 80
services.AddHangfireConfiguration(configuration);                        // line 82
services.AddScoped<OutboxWorker>();                                      // line 103
services.AddScoped<TestRepository>();                                    // line 105

// DbContext with interceptor
services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sql => { sql.MigrationsAssembly(...); });

    options.AddInterceptors(
        sp.GetRequiredService<OutboxInterceptor>());                     // line 124-125
});
```

---

## 3. Application Layer

### 3.1 Email Handler — `Application/EventHandlers/EmailHandler.cs`

```csharp
using Application.Interface.Service.Shared;
using Domain.Events;
using MediatR;

namespace Application.EventHandlers;

public class EmailHandler : INotificationHandler<TestEvent>
{
    private readonly IEmailService _emailService;

    public EmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(
        TestEvent notification,
        CancellationToken cancellationToken)
    {
        await _emailService.SendEmailTestAsync(
            notification.Email,
            "Test Email",
            notification.Message);
    }
}
```

### 3.2 Notification Handler — `Application/EventHandlers/NotificationHandler.cs`

```csharp
using Application.DTO;
using Application.Interface.Service;
using Domain.Events;
using MediatR;

namespace Application.EventHandlers;

public class NotificationHandler : INotificationHandler<TestEvent>
{
    private readonly INotificationService _notificationService;

    public NotificationHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(
        TestEvent notification,
        CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationTestAsync(
           new NotificationDTO
           {
               Title = "Test Notification",
               Body = notification.Message
           });
    }
}
```

### 3.3 Email Service Interface — `Application/Interface/Service/Shared/IEmailService.cs`

```csharp
namespace Application.Interface.Service.Shared
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailTestAsync(string toEmail, string subject, string body);
    }
}
```

### 3.4 Email Service Implementation — `Application/Service/shared/EmailService.cs`

```csharp
using Application.Interface.Service.Shared;
using Domain.Options;
using System.Net.Mail;

namespace Application.Service.shared;

public class EmailService(MailOptions emailOptions) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromMail = emailOptions.FromEmail ?? throw new InvalidOperationException("Email:FromEmail is not configured");
        var fromPassword = emailOptions.FromPassword ?? throw new InvalidOperationException("Email:FromPassword is not configured");

        var theMsg = new MailMessage(fromMail, toEmail, subject, body);
        theMsg.IsBodyHtml = true;

        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new System.Net.NetworkCredential(fromMail, fromPassword),
            EnableSsl = true,
        };

        await smtp.SendMailAsync(theMsg);
    }

    public async Task SendEmailTestAsync(string toEmail, string subject, string body)
    {
        var fromMail = emailOptions.FromEmail ?? throw new InvalidOperationException("Email:FromEmail is not configured");
        var fromPassword = emailOptions.FromPassword ?? throw new InvalidOperationException("Email:FromPassword is not configured");

        var theMsg = new MailMessage(fromMail, toEmail, subject, body);
        theMsg.IsBodyHtml = true;

        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new System.Net.NetworkCredential(fromMail, fromPassword),
            EnableSsl = true,
        };

        await smtp.SendMailAsync(theMsg);
    }
}
```

### 3.5 Notification Service Interface — `Application/Interface/Service/Shared/INotificationService.cs`

```csharp
using Application.DTO;

namespace Application.Interface.Service
{
    public interface INotificationService
    {
        Task<string> SendNotificationAsync(int userId, NotificationDTO notification);
        Task<string> SendNotificationTestAsync(NotificationDTO notification);
        Task<string> SendNotificationListAsync(List<int> userIds, NotificationDTO notification);
        Task<string> SendToTopicAsync(string topic, NotificationDTO notification);
    }
}
```

### 3.6 Notification Service Implementation — `Application/Service/shared/NotificationService.cs`

```csharp
using Application.DTO;
using Application.Interface.Service;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Service;

public class NotificationService : INotificationService
{
    private readonly FirebaseApp _app;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
    {
        _logger = logger;
        var path = configuration["FirebaseConfig:CredentialFilePath"];

        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Firebase credential file path is not configured.");
        if (!File.Exists(path))
            throw new InvalidOperationException($"Firebase credential file not found at: {path}");

        if (FirebaseApp.DefaultInstance == null)
        {
            _app = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(path)
            });
        }
        else
        {
            _app = FirebaseApp.DefaultInstance;
        }
    }

    public async Task<string> SendNotificationAsync(int userId, NotificationDTO notification)
    {
        try
        {
            var message = new Message()
            {
                Token = $"token_for_user_{userId}",
                Notification = new Notification()
                {
                    Title = notification.Title,
                    Body = notification.Body
                },
                Data = notification.ConvertDataToDictionary(),
                Topic = "test_topic"
            };
            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase notification failed for user {UserId}", userId);
            throw new InvalidOperationException("Firebase notification failed.", ex);
        }
    }

    public async Task<string> SendNotificationListAsync(List<int> userIds, NotificationDTO notification)
    {
        try
        {
            var message = new MulticastMessage()
            {
                Tokens = userIds.Select(id => $"token_for_user_{id}").ToList(),
                Notification = new Notification()
                {
                    Title = notification.Title,
                    Body = notification.Body
                },
                Data = notification.ConvertDataToDictionary()
            };
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            return $"Success: {response.SuccessCount} | Failed: {response.FailureCount}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase multicast notification failed");
            throw new InvalidOperationException("Firebase notification failed.", ex);
        }
    }

    public async Task<string> SendToTopicAsync(string topic, NotificationDTO notification)
    {
        try
        {
            var message = new Message()
            {
                Topic = topic,
                Notification = new Notification()
                {
                    Title = notification.Title,
                    Body = notification.Body
                },
                Data = notification.ConvertDataToDictionary()
            };
            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase topic notification failed for topic {Topic}", topic);
            throw new InvalidOperationException("Firebase topic notification failed.", ex);
        }
    }

    public async Task SendAsync(int userId, string title, string body)
    {
        await SendNotificationAsync(userId, new NotificationDTO { Title = title, Body = body });
    }

    public async Task<string> SendNotificationTestAsync(NotificationDTO notification)
    {
        try
        {
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = notification.Title,
                    Body = notification.Body
                },
                Data = notification.ConvertDataToDictionary(),
                Topic = "test_topic"
            };
            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase notification failed for user ");
            throw new InvalidOperationException("Firebase notification failed.", ex);
        }
    }
}
```

### 3.7 Notification DTO — `Application/DTO/Other/NotificationDTO.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Application.DTO;

public record NotificationDTO
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(50, ErrorMessage = "Title cannot exceed 50 characters")]
    public required string Title { get; init; }

    [Required(ErrorMessage = "Body is required")]
    [StringLength(100, ErrorMessage = "Body cannot exceed 100 characters")]
    public required string Body { get; init; }

    public NotificationData? Data { get; init; }

    public Dictionary<string, string>? ConvertDataToDictionary()
    {
        if (Data == null) return null;

        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Data.ClickAction)) dict["click_action"] = Data.ClickAction;
        if (!string.IsNullOrEmpty(Data.Status)) dict["status"] = Data.Status;
        if (!string.IsNullOrEmpty(Data.Timestamp)) dict["timestamp"] = Data.Timestamp;
        return dict;
    }
}

public record NotificationData
{
    public string? ClickAction { get; init; }
    public string? Status { get; init; }
    public string? Timestamp { get; init; }
}
```

### 3.8 DI Registration — `Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs`

```csharp
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Application.Service;
using Application.Service.shared;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}
```

---

## 4. API Layer

### 4.1 Test Controller — `Api/Controllers/TestController.cs`

```csharp
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

    public TestController(
        TestRepository repository,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _repository = repository;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Test()
    {
        await _repository.TestEvent(1, "Hello From Event");
        return Ok();
    }

    [HttpPost("send-notification")]
    [AllowAnonymous]
    public async Task<IActionResult> TestSendNotification()
    {
        string result = await _notificationService.SendNotificationTestAsync(
             new NotificationDTO
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
            "Hello From Email Service");
        return Ok();
    }
}
```

### 4.2 MediatR Registration — `Api/Program.cs` (excerpt)

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(NotificationHandler).Assembly);
});

// Hangfire dashboard + recurring jobs
app.UseHangfireDashboard("/hangfire");
RecurringJobs.Register();
```

---

## 5. Test Examples

The project uses **xUnit**, **Moq**, **FluentAssertions** (UnitTests), and **Microsoft.AspNetCore.Mvc.Testing** + **EF Core InMemory** (IntegrationTests).

### 5.1 Unit Test — OutboxInterceptor

```csharp
using System.Text.Json;
using Domain.Events;
using Domain.Model;
using Domain.Model.Base;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace UnitTests.EventTests;

public class OutboxInterceptorTests
{
    private static DbContextOptions<TestDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SavingChangesAsync_CapturesDomainEvents_AndWritesToOutbox()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var context = new TestDbContext(options);

        var entity = new TestEntity { UserId = 1, Message = "test" };
        entity.AddDomainEvent(new TestEvent(1, "test@test.com", "test"));
        context.Tests.Add(entity);

        var interceptor = new OutboxInterceptor();

        // Act
        await context.SaveChangesAsync();

        // Assert
        var outboxMessages = await context.OutboxMessages.ToListAsync();
        Assert.Single(outboxMessages);
        Assert.Contains(nameof(TestEvent), outboxMessages[0].EventType);
    }

    [Fact]
    public async Task SavingChangesAsync_ClearsDomainEvents_AfterCapture()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var context = new TestDbContext(options);

        var entity = new TestEntity { UserId = 1, Message = "test" };
        entity.AddDomainEvent(new TestEvent(1, "test@test.com", "test"));
        context.Tests.Add(entity);

        var interceptor = new OutboxInterceptor();

        // Act
        await context.SaveChangesAsync();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public async Task SavingChangesAsync_NoEvents_DoesNotWriteOutbox()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var context = new TestDbContext(options);

        var entity = new TestEntity { UserId = 1, Message = "no event" };
        context.Tests.Add(entity);

        var interceptor = new OutboxInterceptor();

        // Act
        await context.SaveChangesAsync();

        // Assert
        var outboxMessages = await context.OutboxMessages.ToListAsync();
        Assert.Empty(outboxMessages);
    }
}

/// <summary>
/// Test DbContext with OutboxInterceptor wired up.
/// </summary>
public class TestDbContext : DbContext
{
    public DbSet<TestEntity> Tests => Set<TestEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
        });
    }
}
```

### 5.2 Unit Test — MediatREventDispatcher

```csharp
using Domain.Events;
using Infrastructure.Dispatching;
using MediatR;
using Moq;

namespace UnitTests.EventTests;

public class MediatREventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_TestEvent_PublishesViaMediatR()
    {
        // Arrange
        var mediatorMock = new Mock<IPublisher>();
        var dispatcher = new MediatREventDispatcher(mediatorMock.Object);

        var testEvent = new TestEvent(1, "test@test.com", "hello");
        var payload = System.Text.Json.JsonSerializer.Serialize(testEvent);

        // Act
        await dispatcher.DispatchAsync("test.v1", payload, CancellationToken.None);

        // Assert
        mediatorMock.Verify(
            x => x.Publish(
                It.Is<TestEvent>(e => e.UserId == 1 && e.Email == "test@test.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_UnknownEventType_DoesNotPublish()
    {
        // Arrange
        var mediatorMock = new Mock<IPublisher>();
        var dispatcher = new MediatREventDispatcher(mediatorMock.Object);

        // Act
        await dispatcher.DispatchAsync("unknown.event", "{}", CancellationToken.None);

        // Assert
        mediatorMock.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

### 5.3 Unit Test — OutboxWorker

```csharp
using Domain.Model;
using Infrastructure.Dispatching;
using Infrastructure.Hangfire;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.EventTests;

public class OutboxWorkerTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task ProcessAsync_ProcessesUnprocessedMessages()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var context = new ApplicationDbContext(options);

        context.OutboxMessages.Add(new OutboxMessage
        {
            EventType = "test.v1",
            Payload = "{}",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var dispatcherMock = new Mock<IEventDispatcher>();
        var worker = new OutboxWorker(context, dispatcherMock.Object);

        // Act
        await worker.ProcessAsync();

        // Assert
        var processed = await context.OutboxMessages
            .Where(x => x.ProcessedAt != null)
            .ToListAsync();
        Assert.Single(processed);
        dispatcherMock.Verify(
            x => x.DispatchAsync("test.v1", "{}", CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenDispatcherThrows_IncrementsRetryCount()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var context = new ApplicationDbContext(options);

        context.OutboxMessages.Add(new OutboxMessage
        {
            EventType = "test.v1",
            Payload = "{}",
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var dispatcherMock = new Mock<IEventDispatcher>();
        dispatcherMock
            .Setup(x => x.DispatchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("test error"));

        var worker = new OutboxWorker(context, dispatcherMock.Object);

        // Act
        await worker.ProcessAsync();

        // Assert
        var message = await context.OutboxMessages.FirstAsync();
        Assert.Equal(1, message.RetryCount);
        Assert.Contains("test error", message.ErrorMessage);
        Assert.Null(message.ProcessedAt);
    }
}
```

### 5.4 Unit Test — EmailHandler

```csharp
using Application.EventHandlers;
using Application.Interface.Service.Shared;
using Domain.Events;
using Moq;

namespace UnitTests.EventTests;

public class EmailHandlerTests
{
    [Fact]
    public async Task Handle_SendsEmail_WithCorrectParameters()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var handler = new EmailHandler(emailServiceMock.Object);
        var testEvent = new TestEvent(1, "user@test.com", "Hello World");

        // Act
        await handler.Handle(testEvent, CancellationToken.None);

        // Assert
        emailServiceMock.Verify(
            x => x.SendEmailTestAsync("user@test.com", "Test Email", "Hello World"),
            Times.Once);
    }
}
```

### 5.5 Unit Test — NotificationHandler

```csharp
using Application.DTO;
using Application.EventHandlers;
using Application.Interface.Service;
using Domain.Events;
using Moq;

namespace UnitTests.EventTests;

public class NotificationHandlerTests
{
    [Fact]
    public async Task Handle_SendsNotification_WithCorrectBody()
    {
        // Arrange
        var notificationServiceMock = new Mock<INotificationService>();
        var handler = new NotificationHandler(notificationServiceMock.Object);
        var testEvent = new TestEvent(1, "test@test.com", "Hello from event");

        // Act
        await handler.Handle(testEvent, CancellationToken.None);

        // Assert
        notificationServiceMock.Verify(
            x => x.SendNotificationTestAsync(
                It.Is<NotificationDTO>(dto =>
                    dto.Title == "Test Notification" &&
                    dto.Body == "Hello from event")),
            Times.Once);
    }
}
```

### 5.6 Unit Test — TestRepository (end-to-end flow with in-memory DB)

```csharp
using Domain.Model;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.EventTests;

public class TestRepositoryTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task TestEvent_SavesEntity_AndRaisesDomainEvent()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var context = new ApplicationDbContext(options);

        var repo = new TestRepository(context);

        // Act
        await repo.TestEvent(42, "Integration Test Message");

        // Assert
        var testEntity = await context.Tests.FirstOrDefaultAsync();
        Assert.NotNull(testEntity);
        Assert.Equal(42, testEntity.UserId);
        Assert.Equal("Integration Test Message", testEntity.Message);

        var outboxMessages = await context.OutboxMessages.ToListAsync();
        Assert.Single(outboxMessages);
    }
}
```

---

## 6. Event Flow Summary

```
POST /api/test
    │
    ▼
TestController.Test()
    │
    ▼
TestRepository.TestEvent(userId, message)
    ├─ Creates TestEntity
    ├─ Adds TestEvent domain event
    └─ Saves (SaveChangesAsync)
          │
          ▼
    OutboxInterceptor.SavingChangesAsync
    ├─ Detects domain events on tracked entities
    ├─ Writes OutboxMessage rows (serialized JSON)
    └─ Clears domain events
          │
          ▼
    SaveChanges completes ─── Response: 200 OK
          │
          ▼
    Hangfire (every minute)
    OutboxWorker.ProcessAsync
    ├─ Queries unprocessed OutboxMessages
    └─ For each:
          ├─ Dispatcher.DispatchAsync(eventType, payload)
          │     └─ MediatREventDispatcher
          │           ├─ Deserializes payload → TestEvent
          │           └─ IPublisher.Publish(TestEvent)
          │                 │
          │                 ├─▶ EmailHandler.Handle
          │                 │     └─ IEmailService.SendEmailTestAsync
          │                 │
          │                 └─▶ NotificationHandler.Handle
          │                       └─ INotificationService.SendNotificationTestAsync
          │
          └─ Marks message as ProcessedAt = now
```

---

## 7. Known Issue

In `Infrastructure/Hangfire/RecurringJobs.cs:12`, the job references `ProcessOutboxMessagesAsync()` but `OutboxWorker` only has `ProcessAsync()`. Fix:

```csharp
// RecurringJobs.cs  →  change to:
job => job.ProcessAsync(),
```

Or rename `ProcessAsync` to `ProcessOutboxMessagesAsync` in `OutboxWorker`.
