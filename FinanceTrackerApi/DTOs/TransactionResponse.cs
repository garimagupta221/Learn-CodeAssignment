using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.DTOs
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public Category Category { get; set; }
        public DateTime Date { get; set; }

    }
}
