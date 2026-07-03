using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Request;
using Application.DTO.Response;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface INotificationService
    {
        /// <summary>
        /// Send a notification to a single device
        /// </summary>
        Task<string> SendNotificationAsync(int userId, NotificationDTO notification);

        /// <summary>
        /// Send a notification to multiple devices
        /// </summary>
        Task<string> SendNotificationListAsync(List<int> userIds, NotificationDTO notification);

        Task<string> SendToTopicAsync(string topic, NotificationDTO notification);
    }

}
