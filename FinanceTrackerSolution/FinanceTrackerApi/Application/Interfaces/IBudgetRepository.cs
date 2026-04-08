using FinanceTrackerApi.Domain.Enums;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Interfaces
{
    public interface IBudgetRepository
    {
        void AddOrUpdate(Budget budget);
        Budget GetBudget(Guid userId, Category category, int month, int year);
    }
}
