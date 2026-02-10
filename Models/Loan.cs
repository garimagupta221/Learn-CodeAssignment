using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankingSystem.Interfaces;
namespace BankingSystem.Models
{
    internal class Loan : ILoan
    {
        public int LoanId { get; private set; }
        public decimal Principal { get; private set; }
        public int LinkedAccountNumber { get; }
        
        public Loan(int loanId, LoanInfo info)
        {
            LoanId = loanId;
            Principal = info.Principal;
            tenureYears = info.TenureYears;
            LinkedAccountNumber = info.LinkedAccountNumber;
            interestRate = DEFAULT_INTEREST_RATE;
        }

        public decimal CalculateInterest()
        {
            return (Principal * interestRate * tenureYears) / 100;
        }

        public decimal CalculateEMI()
        {
            decimal monthlyRate = interestRate / (MONTHS * 100);
            int months = tenureYears * MONTHS;
            decimal numerator = Principal * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months);
            decimal denominator = (decimal)Math.Pow((double)(1 + monthlyRate), months) - 1;

            return numerator / denominator;
        }

        private const decimal DEFAULT_INTEREST_RATE = 10.5m;
        private const int MONTHS = 12;

        private decimal interestRate;
        private int tenureYears;
    }
}