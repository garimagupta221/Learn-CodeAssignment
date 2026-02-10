using BankingSystem.Interfaces;
using BankingSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class Bank
    {
        private readonly ICustomerService _customerService;
        private readonly IAccountService _accountService;
        private readonly ITransferService _transferService;

        public Bank()
        {
            _customerService = new CustomerService();
            _accountService = new AccountService();
            _transferService = new TransferService();
        }

        public Customer CreateCustomer(int id, string name)
        {
            return _customerService.CreateCustomer(id, name);
        }

        public bool CreateAccount(Customer customer)
        {
            return _accountService.CreateAccountForCustomer(customer);
        }

        public bool DeleteAccount(Customer customer, int accountNumber)
        {
            if (customer == null)
            {
                return false;
            }

            if (!_accountService.DeleteAccount(accountNumber))
            {
                return false;
            }

            return customer.RemoveAccount(accountNumber);
        }

        public Customer GetCustomerById(int customerId)
        {
            return _customerService.GetCustomerById(customerId);
        }

        public IAccount FindAccount(int accountNumber)
        {
            return _accountService.FindAccountByNumber(accountNumber);
        }

        public bool Transfer(IAccount sender, IAccount receiver, decimal amount)
        {
            return _transferService.Transfer(sender, receiver, amount);
        }
    }
}