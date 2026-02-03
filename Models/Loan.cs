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
        public double Principal { get; private set; }

        private double interestRate;
        private int tenureYears;

        public Loan(int loanId, double principal, double interestRate, int tenureYears)
        {
            LoanId = loanId;
            Principal = principal;
            this.interestRate = interestRate;
            this.tenureYears = tenureYears;
        }

        public double CalculateInterest()
        {
            return (Principal * interestRate * tenureYears) / 100;
        }

        public double CalculateEMI()
        {
            double monthlyRate = interestRate / (12 * 100);
            int months = tenureYears * 12;

            return (Principal * monthlyRate * Math.Pow(1 + monthlyRate, months)) /
                   (Math.Pow(1 + monthlyRate, months) - 1);
        }
    }
}
