using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/allocations")]
    public class AllocationController : ControllerBase
    {
        private readonly IAllocationService _allocationService;

        public AllocationController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var allocations = await _allocationService.GetAllAsync();
            return Ok(allocations);
        }

        [HttpGet("employee/{id}")]
        public async Task<IActionResult> GetByEmployee(int id)
        {
            var allocations = await _allocationService.GetByEmployeeAsync(id);
            return Ok(allocations);
        }

        [HttpGet("project/{id}")]
        public async Task<IActionResult> GetByProject(int id)
        {
            var allocations = await _allocationService.GetByProjectAsync(id);
            return Ok(allocations);
        }

        [HttpPost]
        public async Task<IActionResult> Allocate([FromBody] CreateAllocationDto dto)
        {
            var allocation = await _allocationService.AllocateAsync(dto);
            return Ok(allocation);
        }

        [HttpPut("{id}/end")]
        public async Task<IActionResult> End(int id)
        {
            await _allocationService.EndAllocationAsync(id);
            return Ok();
        }
    }
}
