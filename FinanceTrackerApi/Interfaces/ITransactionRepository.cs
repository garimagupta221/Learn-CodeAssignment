using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Interfaces
{
    public interface ITransactionRepository
    {
        void Add(Transaction transaction);
        List<Transaction> GetTransactionsByUser(Guid userId);
        Transaction GetTransactionById(Guid transactionId);
        void Delete(Guid transactionId);
    }
}
