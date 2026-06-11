using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface IActivityTagRepository
    {
        Task<ActivityTag?> GetByIdAsync(int id);
        Task<List<ActivityTag>> GetAllAsync();
        Task<ActivityTag> AddAsync(ActivityTag activityTag);
        Task<ActivityTag> UpdateAsync(ActivityTag activityTag);
        Task DeleteAsync(int id);
    }
}
