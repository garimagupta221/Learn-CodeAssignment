using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class TimesheetTagRepository : ITimesheetTagRepository
    {
        private readonly PrmDbContext _context;

        public TimesheetTagRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<TimesheetTag?> GetByIdAsync(int id)
        {
            return await _context.TimesheetTags.FindAsync(id);
        }

        public async Task<List<TimesheetTag>> GetAllAsync()
        {
            return await _context.TimesheetTags.ToListAsync();
        }

        public async Task<TimesheetTag> AddAsync(TimesheetTag timesheetTag)
        {
            _context.TimesheetTags.Add(timesheetTag);
            await _context.SaveChangesAsync();
            return timesheetTag;
        }

        public async Task<TimesheetTag> UpdateAsync(TimesheetTag timesheetTag)
        {
            _context.TimesheetTags.Update(timesheetTag);
            await _context.SaveChangesAsync();
            return timesheetTag;
        }

        public async Task DeleteAsync(int id)
        {
            var timesheetTag = await _context.TimesheetTags.FindAsync(id);
            if (timesheetTag != null)
            {
                _context.TimesheetTags.Remove(timesheetTag);
                await _context.SaveChangesAsync();
            }
        }
    }
}
