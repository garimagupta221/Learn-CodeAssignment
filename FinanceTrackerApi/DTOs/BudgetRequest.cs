using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTracker.Domain.Enums;


namespace FinanceTracker.Application.DTOs
{
    public class BudgetRequest
    {
        public Guid UserId { get; set; }
        public Category Category { get; set; }
        public decimal MonthlyLimit { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
