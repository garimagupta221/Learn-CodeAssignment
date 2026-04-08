using FinanceTrackerApi.Application.DTOs;
using FinanceTrackerApi.Application.Interfaces;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Services
{
    public class BudgetService : IBudgetService
    {
        public BudgetService(IBudgetRepository repo)
        {
            _repo = repo;
        }

        public Budget SetBudget(BudgetRequest request)
        {
            var budget = new Budget
            {
                UserId = request.UserId,
                Category = request.Category,
                MonthlyLimit = request.MonthlyLimit,
                Month = request.Month,
                Year = request.Year
            };

            _repo.AddOrUpdate(budget);
            return budget;
        }

        private readonly IBudgetRepository _repo;

    }
}
