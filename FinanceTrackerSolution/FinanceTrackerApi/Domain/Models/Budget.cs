using FinanceTrackerApi.Domain.Enums;

namespace FinanceTrackerApi.Domain.Models
{
    public class Budget
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Category Category { get; set; }
        public decimal MonthlyLimit { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
