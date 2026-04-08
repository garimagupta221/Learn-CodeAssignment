using FinanceTrackerApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTrackerApi.Controllers
{
    [ApiController]
    [Route("reports")]
    public class ReportController : ControllerBase
    {
        public ReportController(IReportService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public IActionResult GetSummary(
            [FromQuery] Guid userId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            return Ok(_service.GetMonthlySummary(userId, month, year));
        }

        private readonly IReportService _service;

    }
}
