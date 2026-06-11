using PrmServer.DTOs;
using PrmServer.Entities;

namespace PrmServer.Services.Interfaces
{
    public interface ITimesheetService
    {
        Task<List<Timesheet>> GetByEmployeeAsync(int employeeId);
        Task<Timesheet?> GetByIdAsync(int id);
        Task<Timesheet> SubmitAsync(SubmitTimesheetDto dto);
        Task<List<Employee>> GetEmployeesMissingCurrentWeekAsync();
        Task MarkMissedTimesheetsAsync();
    }
}
