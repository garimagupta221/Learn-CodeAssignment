using PrmServer.DTOs;
using PrmServer.Entities;

namespace PrmServer.Services.Interfaces
{
    public interface ITimesheetService
    {
        Task<List<Timesheet>> GetByEmployeeAsync(int employeeId);
        Task<Timesheet?> GetByIdAsync(int id);
        Task<Timesheet> SubmitAsync(SubmitTimesheetDto dto);
        Task<Timesheet> UpdateAsync(int id, UpdateTimesheetDto dto);
        Task ApproveAsync(int id, int approverId);
        Task RejectAsync(int id, string reason, int approverId);
        Task<List<Employee>> GetEmployeesMissingCurrentWeekAsync();
        Task MarkMissedTimesheetsAsync();
    }
}
