using BankingSystem.Interfaces;
using BankingSystem.Models;
using BankingSystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Services
{
    internal class TransferService : ITransferService
    {
        public bool Transfer(IAccount sender, IAccount receiver, decimal amount)
        {
            if (amount <= 0) {
                throw new InvalidAmountException(amount, "transfer");
            }
            if (sender == null) {
                throw new AccountReferenceException("Sender account not found");
            }
            if (receiver == null) {
                throw new AccountReferenceException("Receiver account not found");
            }

            if (sender.AccountNumber == receiver.AccountNumber)
            {
                throw new SameAccountTransferException(sender.AccountNumber);
            }

            sender.Withdraw(amount);
            receiver.Deposit(amount);
            return true;
        }
    }
}