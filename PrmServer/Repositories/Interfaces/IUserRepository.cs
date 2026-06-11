using System.Collections.Generic;
using System.Threading.Tasks;
using PrmServer.Entities;

namespace PrmServer.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
