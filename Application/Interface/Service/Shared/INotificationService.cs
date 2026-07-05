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
