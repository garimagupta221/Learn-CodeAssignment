using BankingSystem.Enums;
using BankingSystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class CurrentAccount : Account
    {
        public override AccountType AccountType => AccountType.Current;
        
        public CurrentAccount(int number, AccountInfo info)
            : base(number, info) { }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0) {
                throw new InvalidAmountException(amount, "withdrawal");
            }

            decimal available = Balance + OverdraftLimit;
            if (available < amount) {
                throw new InsufficientFundsException(AccountNumber, amount, available);
            }

            Balance -= amount;
            return true;
        }

        private const decimal OverdraftLimit = 10000;
    }
}
