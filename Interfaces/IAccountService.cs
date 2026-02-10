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
        bool CreateAccountForCustomer(ICustomer customer);
        bool DeleteAccount(int accountNumber);
        
        IAccount FindAccountByNumber(int accountNumber);
    }
}