using FinanceTrackerApi.Application.DTOs;
using FinanceTrackerApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTrackerApi.Controllers
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
            var transaction = _service.AddTransaction(request);
            return Ok(transaction);
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
