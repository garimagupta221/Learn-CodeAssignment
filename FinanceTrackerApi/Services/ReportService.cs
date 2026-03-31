using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Services
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
