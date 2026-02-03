using BankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Interfaces
{
    internal interface ICustomerService
    {
        Customer CreateCustomer(int id, string name);
        Customer GetCustomerById(int customerId);
    }
}
