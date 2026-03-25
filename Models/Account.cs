using BankingSystem.Enums;
using BankingSystem.Exceptions;
using BankingSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal abstract class Account : IAccount
    {
        public string Username { get; private set; }
        public int CustomerId { get; private set; }
        public int AccountNumber { get; private set; }
        public decimal Balance { get; protected set; }
        public abstract AccountType AccountType { get; }

        public Account(int accountNumber, AccountInfo info)
        {
            if (info == null) {
                throw new ArgumentNullException("info", "Info not found");
            }
            if (accountNumber <= 0) {
                throw new ArgumentOutOfRangeException("accountNumber", "Account number must be positive");
            }
            if (info.CustomerId <= 0) {
                throw new ArgumentOutOfRangeException("customerId", "Customer id must be positive");
            }
            if (string.IsNullOrWhiteSpace(info.Username)) {
                throw new ArgumentOutOfRangeException("username", "Username cannot be empty");
            }
            if (info.InitialBalance < 0) {
                throw new InvalidAmountException(info.InitialBalance, "initial balance");
            }

            AccountNumber = accountNumber;
            CustomerId = info.CustomerId;
            Username = info.Username;
            Balance = info.InitialBalance;
        }

        public virtual bool Deposit(decimal amount)
        {
            if (amount <= 0) {
                throw new InvalidAmountException(amount, "deposit");
            }
            Balance += amount;
            return true;
        }

        public abstract bool Withdraw(decimal amount);
    }
}