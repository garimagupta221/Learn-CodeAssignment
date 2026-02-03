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
        double Principal { get; }

        double CalculateInterest();
        double CalculateEMI();
    }
}
