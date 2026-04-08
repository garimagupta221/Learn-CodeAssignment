using FinanceTrackerApi.Application.Interfaces;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        public void Add(User user)
        {
            _userRecords.Add(user);
        }

        public User GetUserById(Guid userId)
        {
            return _userRecords
                .FirstOrDefault(user => user.Id == userId);
        }

        private readonly List<User> _userRecords = new();
    }
}
