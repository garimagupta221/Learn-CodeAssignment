using BankingSystem.Interfaces;
using BankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Services
{
    internal class AccountService : IAccountService
    {
        public int AccountCount => accounts.Count;

        public AccountService()
        {
            accounts = new List<IAccount>();
        }

        public bool CreateAccountForCustomer(ICustomer customer, string type)
        {
            if (customer == null)
            {
                return false;
            }

            var info = new AccountInfo
            {
                Username = customer.UserName,
                CustomerId = customer.CustomerId,
                InitialBalance = 0m
            };

            int accountNumber = _accountSequence++;
            IAccount account;

            if (type == "Savings")
            {
                account = new SavingsAccount(accountNumber, info);

            }
            else
            {
                account = new CurrentAccount(accountNumber, info);
            }
            accounts.Add(account);
            customer.AddAccount(account);

            return true;
        }

        public bool DeleteAccount(int accountNumber)
        {
            int deleteIndex = -1;
            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].AccountNumber == accountNumber)
                {
                    deleteIndex = index;
                    break;
                }
            }

            if (deleteIndex == -1)
            {
                return false;
            }

            accounts.RemoveAt(deleteIndex);
            return true;
        }

        public IAccount FindAccountByNumber(int accountNumber)
        {
            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].AccountNumber == accountNumber)
                {
                    return accounts[index];
                }
            }

            return null;
        }

        public IAccount GetAccountByUsername(string username)
        {
            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].Username == username)
                {
                    return accounts[index];
                }
            }

            return null;
        }

        private List<IAccount> accounts;
        private int _accountSequence = 100;
    }
}