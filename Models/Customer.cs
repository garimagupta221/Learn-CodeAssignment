using BankingSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class Customer
    {
        public int CustomerId { get; }
        public string Name { get; }

        private List<IAccount> accounts;
        private List<ILoan> loans;

        public int AccountCount => accounts.Count;
        public Customer(int id, string name)
        {
            CustomerId = id;
            Name = name;
            accounts = new List<IAccount>();
            loans = new List<ILoan>();
        }

        public List<IAccount> Accounts => accounts;


        public void AddAccount(IAccount account)
        {
            accounts.Add(account);
        }

        public void AddLoan(ILoan loan)
        {
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

    }
}
