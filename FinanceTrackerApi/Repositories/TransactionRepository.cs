using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceTracker.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        public void Add(Transaction transaction)
        {
            _transactionRecords.Add(transaction);
        }

        public List<Transaction> GetTransactionsByUser(Guid userId)
        {
            return _transactionRecords
                .Where(transaction => transaction.UserId == userId)
                .ToList();
        }

        public Transaction GetTransactionById(Guid transactionId)
        {
            return _transactionRecords
                .FirstOrDefault(transaction => transaction.Id == transactionId);
        }

        public void Delete(Guid transactionId)
        {
            var existingTransaction = GetTransactionById(transactionId);

            if (existingTransaction == null)
                return;

            _transactionRecords.Remove(existingTransaction);
        }
        s
        private readonly List<Transaction> _transactionRecords = new();

    }
}