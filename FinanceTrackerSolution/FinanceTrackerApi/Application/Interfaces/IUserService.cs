using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Interfaces
{
    public interface IUserService
    {
        User CreateUser(string name, string email);
        User GetUserById(Guid id);
    }
}
