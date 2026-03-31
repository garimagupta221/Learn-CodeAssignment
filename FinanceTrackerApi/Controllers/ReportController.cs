using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Controllers
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
