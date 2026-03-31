using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceTracker.Repositories
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