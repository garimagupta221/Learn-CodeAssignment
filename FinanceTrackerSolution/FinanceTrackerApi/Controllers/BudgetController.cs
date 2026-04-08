using FinanceTrackerApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FinanceTrackerApi.Application.DTOs;

namespace FinanceTrackerApi.Controllers
{
    [ApiController]
    [Route("budgets")]
    public class BudgetController : ControllerBase
    {
        public BudgetController(IBudgetService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult Set([FromForm] BudgetRequest request)
        {
            return Ok(_service.SetBudget(request));
        }

        private readonly IBudgetService _service;

    }
}
