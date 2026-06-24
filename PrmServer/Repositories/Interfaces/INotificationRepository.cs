using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(int id);
        Task<List<Notification>> GetAllAsync();
        Task<List<Notification>> GetUnreadByUserIdAsync(int userId);
        Task<List<Notification>> GetByUserIdAsync(int userId);
        Task<Notification> AddAsync(Notification notification);
        Task<Notification> UpdateAsync(Notification notification);
        Task DeleteAsync(int id);
    }
}
