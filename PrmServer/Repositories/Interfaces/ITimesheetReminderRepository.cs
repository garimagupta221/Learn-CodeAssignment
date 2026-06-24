using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface ITimesheetReminderRepository
    {
        /// <summary>
        /// Returns the reminder log for the given user/week, or null if none exists yet.
        /// </summary>
        Task<TimesheetReminderLog?> GetByUserAndWeekAsync(int userId, DateTime weekStart);

        Task<TimesheetReminderLog> AddAsync(TimesheetReminderLog log);

        Task<TimesheetReminderLog> UpdateAsync(TimesheetReminderLog log);
    }
}
