using FinanceTrackerApi.Application.DTOs;
using FinanceTrackerApi.Domain.Enums;
using FinanceTrackerApi.Domain.Models;

namespace FinanceTrackerApi.Application.Interfaces
{
    public interface IBudgetService
    {
        Budget SetBudget(BudgetRequest request);
    }
}
