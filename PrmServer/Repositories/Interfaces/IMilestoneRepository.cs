using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface IMilestoneRepository
    {
        Task<Milestone?> GetByIdAsync(int id);
        Task<List<Milestone>> GetAllAsync();
        Task<List<Milestone>> GetByProjectIdAsync(int projectId);
        Task<Milestone> AddAsync(Milestone milestone);
        Task<Milestone> UpdateAsync(Milestone milestone);
        Task DeleteAsync(int id);
    }
}
