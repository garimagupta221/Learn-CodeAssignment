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
            return Ok(_service.CreateUser(name, email));
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
