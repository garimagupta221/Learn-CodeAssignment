using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Interfaces
{
    public interface IReportService
    {
        MonthlySummaryResponse GetMonthlySummary(Guid userId, int month, int year);

    }
}
