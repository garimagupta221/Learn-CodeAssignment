using PrmServer.Entities;

namespace PrmServer.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateAsync(int userId, string message, string type);
        Task<List<Notification>> GetUnreadAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }
}
