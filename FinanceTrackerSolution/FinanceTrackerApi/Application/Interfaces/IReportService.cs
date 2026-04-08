using FinanceTrackerApi.Application.DTOs;

namespace FinanceTrackerApi.Application.Interfaces
{
    public interface IReportService
    {
        MonthlySummaryResponse GetMonthlySummary(Guid userId, int month, int year);

    }
}
