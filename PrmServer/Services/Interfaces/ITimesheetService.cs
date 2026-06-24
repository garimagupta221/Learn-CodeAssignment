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
        /// <summary>
        /// Creates MISSED timesheet records for every allocated employee who did not submit
        /// a timesheet for the previous week. Returns the user IDs of affected employees.
        /// </summary>
        Task<List<int>> MarkMissedTimesheetsAsync();
    }
}
