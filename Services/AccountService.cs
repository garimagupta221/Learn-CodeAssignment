using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Exceptions;
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
            if (customer == null) {
                throw new ArgumentNullException("Customer not found");
            }
            if (string.IsNullOrWhiteSpace(type)) {
                throw new InvalidAccountTypeException(type);
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
            else if (type == "Current")
            {
                account = new CurrentAccount(accountNumber, info);
            }
            else
            {
                throw new InvalidAccountTypeException(type);
            }
            accounts.Add(account);
            customer.AddAccount(account);

            return true;
        }

        public bool DeleteAccount(int accountNumber)
        {
            if (accountNumber <= 0) {
                throw new ArgumentOutOfRangeException("accountNumber", "Account number must be positive");
            }

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
                throw new AccountNotFoundException(accountNumber);
            }

            accounts.RemoveAt(deleteIndex);
            return true;
        }

        public IAccount FindAccountByNumber(int accountNumber)
        {
            if (accountNumber <= 0) {
                throw new ArgumentOutOfRangeException("accountNumber", "Account number must be positive");
            }

            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].AccountNumber == accountNumber)
                {
                    return accounts[index];
                }
            }

            throw new AccountNotFoundException(accountNumber);
        }

        private List<IAccount> accounts;
        private int _accountSequence = 100;
    }
}