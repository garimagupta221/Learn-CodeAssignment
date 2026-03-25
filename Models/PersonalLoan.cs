using BankingSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Models
{
    internal class PersonalLoan : Loan
    {
        public override LoanType LoanType => LoanType.Personal;

        public PersonalLoan(int id, LoanInfo info) : base(id, info) { }

        public override decimal CalculateInterest()
        {
            return (Principal * Rate * TenureYears) / 100;
        }

        public override decimal CalculateEMI()
        {
            return (Principal + CalculateInterest()) / (TenureYears * 12);
        }

        private const decimal Rate = 12.5m;
    }
}
