using BankingSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class Bank : ICustomerService, IAccountService
    {
        private List<IAccount> accounts;
        private List<Customer> customers;

        public Bank()
        {
            accounts = new List<IAccount>();
            customers = new List<Customer>();
        }

        public Customer CreateCustomer(int id, string name)
        {
            Customer customer = new Customer(id, name);
            customers.Add(customer);
            return customer;
        }

        public bool CreateAccountForCustomer(Customer customer)
        {
            if (customer == null)
                return false;

            int accountNumber = 1000 + accounts.Count;
            IAccount account = new Account(customer.Name, accountNumber, customer.CustomerId, 0.0);

            accounts.Add(account);
            customer.AddAccount(account);

            return true;
        }

        public bool DeleteAccount(int accountNumber)
        {
            int deleteIndex = -1;
            IAccount account = null;

            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].AccountNumber == accountNumber)
                {
                    deleteIndex = index;
                    account = accounts[index];
                    break;
                }
            }

            if (deleteIndex == -1)
                return false;

            accounts.RemoveAt(deleteIndex);
            Customer owner = GetCustomerById(account.CustomerId);
            if (owner != null)
            {
                owner.RemoveAccount(accountNumber);
            }
            return true;
        }

        public Customer GetCustomerById(int customerId)
        {
            for (int index = 0; index < customers.Count; index++)
            {
                if (customers[index].CustomerId == customerId)
                    return customers[index];
            }
            return null;
        }

        public IAccount FindAccountByNumber(int accountNumber)
        {
            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].AccountNumber == accountNumber)
                    return accounts[index];
            }
            return null;
        }

        public IAccount GetAccountByUsername(string username)
        {
            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].Username == username)
                    return accounts[index];
            }
            return null;
        }

        public int AccountCount
        {
            get { return accounts.Count; }
        }

        public IAccount GetAccount(int index)
        {
            if (index < 0 || index >= accounts.Count)
                return null;

            return accounts[index];
        }

    }
}
