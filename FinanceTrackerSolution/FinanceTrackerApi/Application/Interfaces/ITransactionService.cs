using FinanceTrackerApi.Application.DTOs;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Interfaces
{
    public interface ITransactionService
    {
        Transaction AddTransaction(CreateTransactionRequest request);
        List<Transaction> GetTransactions(TransactionFilter filter);
        void DeleteTransaction(Guid id);
    }
}
