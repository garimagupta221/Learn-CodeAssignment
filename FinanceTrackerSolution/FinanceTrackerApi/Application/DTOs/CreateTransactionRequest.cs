using FinanceTrackerApi.Domain.Enums;

namespace FinanceTrackerApi.Application.DTOs
{
    public class CreateTransactionRequest
    {
        public Guid UserId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public Category Category { get; set; }
        public DateTime Date { get; set; }

    }
}
