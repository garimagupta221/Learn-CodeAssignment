using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.DTOs
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
