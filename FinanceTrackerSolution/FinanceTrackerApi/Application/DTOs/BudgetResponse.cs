using FinanceTrackerApi.Domain.Enums;

namespace FinanceTrackerApi.Application.DTOs
{
    public class BudgetResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Category Category { get; set; }
        public decimal MonthlyLimit { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

    }
}
