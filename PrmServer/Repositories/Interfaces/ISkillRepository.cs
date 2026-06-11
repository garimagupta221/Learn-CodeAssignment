using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface ISkillRepository
    {
        Task<Skill?> GetByIdAsync(int id);
        Task<Skill?> GetByNameAsync(string name);
        Task<List<Skill>> GetAllAsync();
        Task<Skill> AddAsync(Skill skill);
        Task<Skill> UpdateAsync(Skill skill);
        Task DeleteAsync(int id);
    }
}
