using FinanceTrackerApi.Application.DTOs;
using FinanceTrackerApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace FinanceTrackerApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateUserRequest request)
        {
            var user = _service.CreateUser(request.Name, request.Email);
            return Ok(user);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            try
            {
                return Ok(_service.GetUserById(id));
            }
            catch (Exception exception)
            {
                return NotFound(exception.Message);
            }
        }

        private readonly IUserService _service;

    }
}
