    using FinanceTrackerApi.Domain.Models;

    namespace FinanceTrackerApi.Application.Interfaces
    {
        public interface IUserRepository
        {
            void Add(User user);
            User GetUserById(Guid id);
        }
    }
