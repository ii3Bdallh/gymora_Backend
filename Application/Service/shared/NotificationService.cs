using Application.DTO;
using Application.Interface.Service;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Service
{
    public class NotificationService : INotificationService
    {        private readonly FirebaseApp _app;
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

        public async Task<string> SendNotificationAsync(
            int userId,
            NotificationDTO notification)
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

        public async Task<string> SendNotificationListAsync(
            List<int> userIds,
            NotificationDTO notification)
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
            await SendNotificationAsync(
                userId,
                new NotificationDTO { Title = title, Body = body });
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
}
