using FinanceTrackerApi.Application.Interfaces;
using FinanceTrackerApi.Domain.Enums;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        public void AddOrUpdate(Budget budget)
        {
            var existingBudget = GetBudget(
                budget.UserId,
                budget.Category,
                budget.Month,
                budget.Year);

            if (existingBudget != null)
            {
                existingBudget.MonthlyLimit = budget.MonthlyLimit;
                return;
            }

            _budgetRecords.Add(budget);
        }

        public Budget GetBudget(Guid userId, Category category, int month, int year)
        {
            return _budgetRecords.FirstOrDefault(budget =>
                budget.UserId == userId &&
                budget.Category == category &&
                budget.Month == month &&
                budget.Year == year);
        }

        private readonly List<Budget> _budgetRecords = new();
    }
}
