using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Services
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
