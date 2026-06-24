using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class TimesheetRepository : ITimesheetRepository
    {
        private readonly PrmDbContext _context;

        public TimesheetRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<Timesheet?> GetByIdAsync(int id)
        {
            return await _context.Timesheets
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.TimesheetTags)
                    .ThenInclude(tt => tt.ActivityTag)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Timesheet>> GetAllAsync()
        {
            return await _context.Timesheets
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.TimesheetTags)
                    .ThenInclude(tt => tt.ActivityTag)
                .ToListAsync();
        }

        public async Task<List<Timesheet>> GetByUserIdAsync(int userId)
        {
            return await _context.Timesheets
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.TimesheetTags)
                    .ThenInclude(tt => tt.ActivityTag)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<Timesheet> AddAsync(Timesheet timesheet)
        {
            _context.Timesheets.Add(timesheet);
            await _context.SaveChangesAsync();
            return timesheet;
        }

        public async Task<Timesheet> UpdateAsync(Timesheet timesheet)
        {
            _context.Timesheets.Update(timesheet);
            await _context.SaveChangesAsync();
            return timesheet;
        }

        public async Task DeleteAsync(int id)
        {
            var timesheet = await _context.Timesheets.FindAsync(id);
            if (timesheet != null)
            {
                _context.Timesheets.Remove(timesheet);
                await _context.SaveChangesAsync();
            }
        }
    }
}
