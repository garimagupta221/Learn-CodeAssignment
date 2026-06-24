using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Notification> CreateAsync(int userId, string message, string type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.AddAsync(notification);
        }

        public Task<List<Notification>> GetUnreadAsync(int userId)
        {
            return _notificationRepository.GetUnreadByUserIdAsync(userId);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId)
                ?? throw new KeyNotFoundException($"Notification {notificationId} not found.");

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _notificationRepository.GetUnreadByUserIdAsync(userId);

            foreach (var notification in unread)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }
    }
}
