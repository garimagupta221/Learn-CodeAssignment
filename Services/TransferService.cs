using BankingSystem.Interfaces;
using BankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Services
{
    internal class TransferService : ITransferService
    {
        public bool Transfer(IAccount sender, IAccount receiver, double amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Invalid transfer amount");
                return false;
            }

            if (sender == null)
            {
                Console.WriteLine("Sender account not found");
                return false;
            }

            if (receiver == null)
            {
                Console.WriteLine("Recipient account not found");
                return false;
            }

            if (sender.Balance < amount)
            {
                Console.WriteLine("Insufficient balance");
                return false;
            }

            if (!sender.Withdraw(amount))
            {
                Console.WriteLine("Withdrawal failed");
                return false;
            }

            receiver.Deposit(amount);
            return true;
        }
    }
}
