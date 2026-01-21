using System.Collections.Generic;
using System.Threading.Tasks;
using MarketService.Models;

namespace MarketService.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(int userId, string title, string message, string? targetUrl = null);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int count = 20);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
