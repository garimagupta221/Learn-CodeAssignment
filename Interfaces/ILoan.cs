using BankingSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystem.Interfaces
{
    internal interface ILoan
    {
        int LoanId { get; }
        decimal Principal { get; }
        int LinkedAccountNumber { get; }
        LoanType LoanType { get; }

        decimal CalculateInterest();
        decimal CalculateEMI();
    }
}