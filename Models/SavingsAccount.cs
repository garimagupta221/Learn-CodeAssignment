using BankingSystem.Enums;
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
            if (amount <= 0)
            {
                return false;
            }

            if (Balance < amount)
            {
                return false;
            }

            Balance -= amount;
            return true;
        }
    }
}
