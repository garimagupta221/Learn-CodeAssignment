using BankingSystem.Interfaces;
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
            CustomerId = id;
            UserName = userName;
            accounts = new List<IAccount>();
            loans = new List<ILoan>();
        }
        
        public void AddAccount(IAccount account)
        {
            if (account == null)
            {
                return;
            }

            accounts.Add(account);
        }

        public void AddLoan(ILoan loan)
        {
            if(loan == null)
            {
                return;
            }

            loans.Add(loan);
        }

        public bool RemoveAccount(int accountNumber)
        {
            for (int index = 0; index < accounts.Count; index++)
            {
                if (accounts[index].AccountNumber == accountNumber)
                {
                    accounts.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        public IAccount GetAccountByNumber(int accountNumber)
        {
            foreach (var account in accounts)
            {
                if (account.AccountNumber == accountNumber)
                {
                    return account;
                }
            }

            return null;
        }

        private List<IAccount> accounts;
        private List<ILoan> loans;
    }
}