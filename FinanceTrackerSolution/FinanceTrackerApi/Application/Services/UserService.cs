using FinanceTrackerApi.Application.Interfaces;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Services
{
    public class UserService : IUserService
    {
        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public User CreateUser(string name, string email)
        {
            var user = new User
            {
                Name = name,
                Email = email
            };

            _repo.Add(user);
            return user;
        }

        public User GetUserById(Guid id)
        {
            var user = _repo.GetUserById(id);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            return user;
        }

        private readonly IUserRepository _repo;

    }
}
