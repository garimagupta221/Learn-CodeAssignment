using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class ActivityTagRepository : IActivityTagRepository
    {
        private readonly PrmDbContext _context;

        public ActivityTagRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityTag?> GetByIdAsync(int id)
        {
            return await _context.ActivityTags.FindAsync(id);
        }

        public async Task<List<ActivityTag>> GetAllAsync()
        {
            return await _context.ActivityTags.ToListAsync();
        }

        public async Task<ActivityTag> AddAsync(ActivityTag activityTag)
        {
            _context.ActivityTags.Add(activityTag);
            await _context.SaveChangesAsync();
            return activityTag;
        }

        public async Task<ActivityTag> UpdateAsync(ActivityTag activityTag)
        {
            _context.ActivityTags.Update(activityTag);
            await _context.SaveChangesAsync();
            return activityTag;
        }

        public async Task DeleteAsync(int id)
        {
            var activityTag = await _context.ActivityTags.FindAsync(id);
            if (activityTag != null)
            {
                _context.ActivityTags.Remove(activityTag);
                await _context.SaveChangesAsync();
            }
        }
    }
}
