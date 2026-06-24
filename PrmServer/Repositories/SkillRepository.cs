using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Repositories
{
    public class SkillRepository : ISkillRepository
    {
        private readonly PrmDbContext _context;

        public SkillRepository(PrmDbContext context)
        {
            _context = context;
        }

        public async Task<Skill?> GetByIdAsync(int id)
        {
            return await _context.Skills.FindAsync(id);
        }

        public async Task<Skill?> GetByNameAsync(string name)
        {
            return await _context.Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
        }

        public async Task<List<Skill>> GetAllAsync()
        {
            return await _context.Skills.ToListAsync();
        }

        public async Task<Skill> AddAsync(Skill skill)
        {
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task<Skill> UpdateAsync(Skill skill)
        {
            _context.Skills.Update(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        public async Task DeleteAsync(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill != null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
            }
        }
    }
}
