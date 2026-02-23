using BankingSystem.Enums;
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
        public abstract AccountType AccountType { get; }

        public Account(int accountNumber, AccountInfo info)
        {
            AccountNumber = accountNumber;
            CustomerId = info.CustomerId;
            Username = info.Username;
            Balance = info.InitialBalance;
        }

        public virtual bool Deposit(decimal amount)
        {
            if (amount <= 0){
                return false;
            }

            Balance += amount;
            return true;
        }

        public abstract bool Withdraw(decimal amount);
    }
}