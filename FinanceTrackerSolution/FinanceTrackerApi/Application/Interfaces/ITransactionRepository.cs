using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Interfaces
{
    public interface ITransactionRepository
    {
        void Add(Transaction transaction);
        List<Transaction> GetTransactionsByUser(Guid userId);
        Transaction GetTransactionById(Guid transactionId);
        void Delete(Guid transactionId);
    }
}
