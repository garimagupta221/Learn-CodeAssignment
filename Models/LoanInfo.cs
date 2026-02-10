using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class LoanInfo
    {
        public decimal Principal { get; set; }
        public int TenureYears { get; set; }
        public int LinkedAccountNumber { get; set; }

    }
}