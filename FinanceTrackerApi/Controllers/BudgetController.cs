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
    [Route("budgets")]
    public class BudgetController : ControllerBase
    {
        public BudgetController(IBudgetService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult Set([FromBody] BudgetRequest request)
        {
            return Ok(_service.SetBudget(request));
        }
        
        private readonly IBudgetService _service;

    }
}
