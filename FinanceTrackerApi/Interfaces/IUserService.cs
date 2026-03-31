using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Interfaces
{
    public interface IUserService
    {
        User CreateUser(string name, string email);
        User GetUserById(Guid id);
    }
}
