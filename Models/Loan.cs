using BankingSystem.Enums;
using BankingSystem.Exceptions;
using BankingSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BankingSystem.Models
{
    internal abstract class Loan : ILoan
    {
        public int LoanId { get; private set; }
        public decimal Principal { get; private set; }
        public int LinkedAccountNumber { get; }
        public int TenureYears { get; }
        public abstract LoanType LoanType { get; }

        public Loan(int loanId, LoanInfo info)
        {
            if (info == null) {
                throw new ArgumentNullException("info", "Info not found");
            }
            if (loanId <= 0) {
                throw new ArgumentOutOfRangeException("loanId", "Loan id must be positive");
            }
            if (info.Principal <= 0) {
                throw new InvalidLoanException("Loan principal must be positive.");
            }
            if (info.TenureYears <= 0) {
                throw new InvalidLoanException("Loan tenure must be positive.");
            }
            if (info.LinkedAccountNumber <= 0) {
                throw new InvalidLoanException("Linked account number must be positive.");
            }

            LoanId = loanId;
            Principal = info.Principal;
            TenureYears = info.TenureYears;
            LinkedAccountNumber = info.LinkedAccountNumber;
            interestRate = DEFAULT_INTEREST_RATE;
        }

        public abstract decimal CalculateInterest();
        public abstract decimal CalculateEMI();

        private const decimal DEFAULT_INTEREST_RATE = 10.5m;
        private const int MONTHS = 12;

        private decimal interestRate;
    }
}