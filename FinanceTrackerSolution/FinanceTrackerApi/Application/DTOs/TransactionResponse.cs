using FinanceTrackerApi.Domain.Enums;

namespace FinanceTrackerApi.Application.DTOs
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Type { get; set; }
        public decimal Amount { get; set; }
        public Category Category { get; set; }
        public DateTime Date { get; set; }

    }
}
