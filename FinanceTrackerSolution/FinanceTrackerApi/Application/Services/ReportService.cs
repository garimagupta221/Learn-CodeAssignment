using FinanceTrackerApi.Application.DTOs;
using FinanceTrackerApi.Application.Interfaces;
using FinanceTrackerApi.Domain.Enums;

namespace FinanceTrackerApi.Application.Services
{
    public class ReportService : IReportService
    {
        public ReportService(ITransactionRepository repo)
        {
            _repo = repo;
        }

        public MonthlySummaryResponse GetMonthlySummary(Guid userId, int month, int year)
        {
            var transactions = _repo.GetTransactionsByUser(userId);

            var monthlyTransactions = transactions
                .Where(transaction =>
                    transaction.Date.Month == month &&
                    transaction.Date.Year == year);

            var totalIncome = monthlyTransactions
                .Where(transaction => transaction.Type == TransactionType.Income)
                .Sum(transaction => transaction.Amount);

            var totalExpense = monthlyTransactions
                .Where(transaction => transaction.Type == TransactionType.Expense)
                .Sum(transaction => transaction.Amount);

            return new MonthlySummaryResponse
            {
                Month = month,
                Year = year,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Savings = totalIncome - totalExpense
            };
        }

        private readonly ITransactionRepository _repo;

    }
}
