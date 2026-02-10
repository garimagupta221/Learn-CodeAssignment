using BankingSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class Account: IAccount
    {
        public string Username { get; private set; }
        public int CustomerId { get; private set; }
        public int AccountNumber { get; private set; }
        public decimal Balance { get; private set; }

        public Account(int accountNumber, AccountInfo info)
        {
            AccountNumber = accountNumber;
            CustomerId = info.CustomerId;
            Username = info.Username;
            Balance = info.InitialBalance;
        }

        public bool Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Amount is invalid");
                return false;
            }

            Balance += amount;
            return true;
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Amount is invalid");
                return false;
            }

            if (Balance < amount)
            {
                Console.WriteLine("Account balance is insufficient");
                return false;
            }

            Balance -= amount;
            return true;
        }
    }
}