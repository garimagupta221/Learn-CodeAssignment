using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class MilestoneRepository : IMilestoneRepository
    {
        private readonly PrmDbContext _context;

        public MilestoneRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<Milestone?> GetByIdAsync(int id)
        {
            return await _context.Milestones.FindAsync(id);
        }

        public async Task<List<Milestone>> GetAllAsync()
        {
            return await _context.Milestones.ToListAsync();
        }

        public async Task<List<Milestone>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Milestones
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<Milestone> AddAsync(Milestone milestone)
        {
            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();
            return milestone;
        }

        public async Task<Milestone> UpdateAsync(Milestone milestone)
        {
            _context.Milestones.Update(milestone);
            await _context.SaveChangesAsync();
            return milestone;
        }

        public async Task DeleteAsync(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);
            if (milestone != null)
            {
                _context.Milestones.Remove(milestone);
                await _context.SaveChangesAsync();
            }
        }
    }
}
