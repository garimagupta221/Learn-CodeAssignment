using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.DTOs
{
    public class TransactionFilter
    {
        public Guid UserId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Category Category { get; set; }

    }
}
