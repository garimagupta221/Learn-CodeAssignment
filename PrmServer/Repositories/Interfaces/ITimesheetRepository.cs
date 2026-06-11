using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface ITimesheetRepository
    {
        Task<Timesheet?> GetByIdAsync(int id);
        Task<List<Timesheet>> GetAllAsync();
        Task<List<Timesheet>> GetByUserIdAsync(int userId);
        Task<Timesheet> AddAsync(Timesheet timesheet);
        Task<Timesheet> UpdateAsync(Timesheet timesheet);
        Task DeleteAsync(int id);
    }
}
