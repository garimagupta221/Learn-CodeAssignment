using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface ITimesheetTagRepository
    {
        Task<TimesheetTag?> GetByIdAsync(int id);
        Task<List<TimesheetTag>> GetAllAsync();
        Task<TimesheetTag> AddAsync(TimesheetTag timesheetTag);
        Task<TimesheetTag> UpdateAsync(TimesheetTag timesheetTag);
        Task DeleteAsync(int id);
    }
}
