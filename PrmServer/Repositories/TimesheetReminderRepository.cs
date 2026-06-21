using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class TimesheetReminderRepository : ITimesheetReminderRepository
    {
        private readonly PrmDbContext _context;

        public TimesheetReminderRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<TimesheetReminderLog?> GetByUserAndWeekAsync(int userId, DateTime weekStart)
        {
            return await _context.TimesheetReminderLogs
                .FirstOrDefaultAsync(l => l.UserId == userId && l.WeekStart.Date == weekStart.Date);
        }

        public async Task<TimesheetReminderLog> AddAsync(TimesheetReminderLog log)
        {
            _context.TimesheetReminderLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<TimesheetReminderLog> UpdateAsync(TimesheetReminderLog log)
        {
            _context.TimesheetReminderLogs.Update(log);
            await _context.SaveChangesAsync();
            return log;
        }
    }
}
