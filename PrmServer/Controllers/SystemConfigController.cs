using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class SystemConfigController : ControllerBase
    {
        private readonly ISystemConfigService _configService;

        public SystemConfigController(ISystemConfigService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Retrieves all system configurations.
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var config = _configService.GetAll();
            return Ok(config);
        }

        /// <summary>
        /// Sets a system configuration value.
        /// </summary>
        [HttpPut]
        public IActionResult Set([FromBody] SetConfigDto dto)
        {
            _configService.Set(dto.Key, dto.Value);
            return Ok();
        }
    }
}
