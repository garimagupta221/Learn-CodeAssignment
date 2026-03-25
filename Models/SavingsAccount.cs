using BankingSystem.Enums;
using BankingSystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class SavingsAccount : Account
    {
        public override AccountType AccountType => AccountType.Savings;

        public SavingsAccount(int number, AccountInfo info)
            : base(number, info) { }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0) {
                throw new InvalidAmountException(amount, "withdrawal");
            }
            if (Balance < amount) {
                throw new InsufficientFundsException(AccountNumber, amount, Balance);
            }

            Balance -= amount;
            return true;
        }
    }
}
