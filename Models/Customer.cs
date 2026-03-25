using BankingSystem.Interfaces;
using BankingSystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class Customer: ICustomer
    {
        public int CustomerId { get; }
        public string UserName { get; }
        public int AccountCount => accounts.Count;
        public IReadOnlyList<IAccount> Accounts => accounts;
        public IReadOnlyList<ILoan> Loans => loans;

        public Customer(int id, string userName)
        {
            if (id <= 0) {
                throw new ArgumentOutOfRangeException("id", "Customer id must be positive");
            }
            if (string.IsNullOrWhiteSpace(userName)) {
                throw new ArgumentOutOfRangeException("userName", "Username cannot be empty");
            }

            CustomerId = id;
            UserName = userName;
            accounts = new List<IAccount>();
            loans = new List<ILoan>();
        }
        
        public void AddAccount(IAccount account)
        {
            if (account == null) {
                throw new ArgumentNullException("Account not found");
            }
            accounts.Add(account);
        }

        public void AddLoan(ILoan loan)
        {
            if (loan == null) {
                throw new ArgumentNullException("Loan not found");
            }
            loans.Add(loan);
        }

        public void RemoveAccount(int accountNumber)
        {
            if (accountNumber <= 0) {
                throw new ArgumentOutOfRangeException("accountNumber", "Account number must be positive");
            }

            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].AccountNumber == accountNumber)
                {
                    accounts.RemoveAt(index);
                    return;
                }
            }

            throw new AccountNotFoundException(accountNumber);
        }

        public IAccount GetAccountByNumber(int accountNumber)
        {
            if (accountNumber <= 0) {
                throw new ArgumentOutOfRangeException("accountNumber", "Account number must be positive");
            }

            foreach (var account in accounts)
            {
                if (account.AccountNumber == accountNumber)
                {
                    return account;
                }
            }

            throw new AccountNotFoundException(accountNumber);
        }

        private List<IAccount> accounts;
        private List<ILoan> loans;
    }
}