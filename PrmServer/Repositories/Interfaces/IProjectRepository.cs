using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(int id);
        Task<List<Project>> GetAllAsync();
        Task<List<Project>> GetByManagerIdAsync(int managerId);
        Task<Project> AddAsync(Project project);
        Task<Project> UpdateAsync(Project project);
        Task DeleteAsync(int id);
    }
}
