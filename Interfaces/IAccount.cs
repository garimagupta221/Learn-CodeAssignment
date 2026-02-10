using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Interfaces
{
    internal interface IAccount
    {
        int CustomerId { get; }
        int AccountNumber { get; }
        string Username { get; }
        decimal Balance { get; }

        bool Deposit(decimal amount);
        bool Withdraw(decimal amount);
    }
}