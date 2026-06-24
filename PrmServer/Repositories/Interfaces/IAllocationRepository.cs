using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface IAllocationRepository
    {
        Task<Allocation?> GetByIdAsync(int id);
        Task<List<Allocation>> GetAllAsync();
        Task<List<Allocation>> GetByUserIdAsync(int userId);
        Task<List<Allocation>> GetByProjectIdAsync(int projectId);
        Task<Allocation> AddAsync(Allocation allocation);
        Task<Allocation> UpdateAsync(Allocation allocation);
        Task DeleteAsync(int id);
    }
}
