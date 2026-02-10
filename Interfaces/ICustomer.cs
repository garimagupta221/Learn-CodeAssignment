using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Interfaces
{
    internal interface ICustomer
    {
        int CustomerId { get; }
        string UserName { get; }
        
        void AddAccount(IAccount account);

    }
}