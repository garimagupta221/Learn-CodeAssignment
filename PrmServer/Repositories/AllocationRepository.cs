using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class AllocationRepository : IAllocationRepository
    {
        private readonly PrmDbContext _context;

        public AllocationRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<Allocation?> GetByIdAsync(int id)
        {
            return await _context.Allocations
                .Include(a => a.User)
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Allocation>> GetAllAsync()
        {
            return await _context.Allocations
                .Include(a => a.User)
                .Include(a => a.Project)
                .ToListAsync();
        }

        public async Task<List<Allocation>> GetByUserIdAsync(int userId)
        {
            return await _context.Allocations
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Allocation>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Allocations
                .Include(a => a.User)
                .Include(a => a.Project)
                .Where(a => a.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<Allocation> AddAsync(Allocation allocation)
        {
            _context.Allocations.Add(allocation);
            await _context.SaveChangesAsync();
            return allocation;
        }

        public async Task<Allocation> UpdateAsync(Allocation allocation)
        {
            _context.Allocations.Update(allocation);
            await _context.SaveChangesAsync();
            return allocation;
        }

        public async Task DeleteAsync(int id)
        {
            var allocation = await _context.Allocations.FindAsync(id);
            if (allocation != null)
            {
                _context.Allocations.Remove(allocation);
                await _context.SaveChangesAsync();
            }
        }
    }
}
