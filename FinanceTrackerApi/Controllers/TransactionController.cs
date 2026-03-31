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
    [Route("transactions")]
    public class TransactionController : ControllerBase
    {
        public TransactionController(ITransactionService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult Add([FromBody] CreateTransactionRequest request)
        {
            return Ok(_service.AddTransaction(request));
        }

        [HttpGet]
        public IActionResult Get([FromQuery] TransactionFilter filter)
        {
            return Ok(_service.GetTransactions(filter));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _service.DeleteTransaction(id);
            return Ok();
        }
        
        private readonly ITransactionService _service;

    }
}