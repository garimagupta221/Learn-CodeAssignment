using BankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Interfaces
{
    internal interface IAccountService
    {
        bool CreateAccountForCustomer(Customer customer);
        bool DeleteAccount(int accountNumber);

        IAccount FindAccountByNumber(int accountNumber);
        IAccount GetAccountByUsername(string username);

        int AccountCount { get; }
        IAccount GetAccount(int index);
    }
}
