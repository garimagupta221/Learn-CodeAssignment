using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly PrmDbContext _context;

        public ProjectRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .ToListAsync();
        }

        public async Task<List<Project>> GetByManagerIdAsync(int managerId)
        {
            return await _context.Projects
                .Include(p => p.Manager)
                .Where(p => p.ManagerId == managerId)
                .ToListAsync();
        }

        public async Task<Project> AddAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }
    }
}
