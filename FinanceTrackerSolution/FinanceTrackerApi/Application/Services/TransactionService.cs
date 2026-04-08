using FinanceTrackerApi.Application.DTOs;
using FinanceTrackerApi.Application.Interfaces;
using FinanceTrackerApi.Domain.Enums;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Services
{
    public class TransactionService : ITransactionService
    {
        public TransactionService(
            ITransactionRepository repo,
            IBudgetRepository budgetRepo,
            INotificationService notify)
        {
            _repo = repo;
            _budgetRepo = budgetRepo;
            _notify = notify;
        }

        public Transaction AddTransaction(CreateTransactionRequest request)
        {
            var transaction = new Transaction
            {
                UserId = request.UserId,
                Type = request.Type,
                Amount = request.Amount,
                Category = request.Category,
                Date = request.Date
            };

            _repo.Add(transaction);

            if (request.Type == TransactionType.Expense)
            {
                CheckBudget(request.UserId, request.Category, request.Date);
            }

            return transaction;
        }

        private void CheckBudget(Guid userId, Category category, DateTime date)
        {
            var budget = _budgetRepo.GetBudget(userId, category, date.Month, date.Year);
            if (budget == null)
            {
                return;
            }

            var transactions = _repo.GetTransactionsByUser(userId);

            var spent = transactions
                .Where(transaction =>
                    transaction.Date.Month == date.Month &&
                    transaction.Date.Year == date.Year &&
                    transaction.Category == category &&
                    transaction.Type == TransactionType.Expense)
                .Sum(transaction => transaction.Amount);

            if (spent > budget.MonthlyLimit)
            {
                _notify.SendNotification("Budget exceeded");
            }

        }

        public List<Transaction> GetTransactions(TransactionFilter filter)
        {
            var data = _repo.GetTransactionsByUser(filter.UserId);

            return data
                .Where(transaction =>
                    transaction.Date >= filter.From &&
                    transaction.Date <= filter.To &&
                    transaction.Category == filter.Category)
                .ToList();
        }

        public void DeleteTransaction(Guid id)
        {
            var transaction = _repo.GetTransactionById(id);

            if (transaction == null)
            {
                throw new Exception("Transaction not found");
            }

            _repo.Delete(id);
        }

        private readonly ITransactionRepository _repo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly INotificationService _notify;

    }
}
