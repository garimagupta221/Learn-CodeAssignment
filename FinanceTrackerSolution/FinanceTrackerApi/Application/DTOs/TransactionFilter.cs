using FinanceTrackerApi.Domain.Enums;

namespace FinanceTrackerApi.Application.DTOs
{
    public class TransactionFilter
    {
        public Guid UserId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Category Category { get; set; }

    }
}
