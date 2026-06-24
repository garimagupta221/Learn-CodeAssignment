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

        /// <summary>
        /// Retrieves all allocations.
        /// </summary>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetAll()
        {
            var allocations = await _allocationService.GetAllAsync();
            return Ok(allocations);
        }

        /// <summary>
        /// Retrieves allocations for a specific employee.
        /// </summary>
        [HttpGet("employee/{id}")]
        public async Task<IActionResult> GetByEmployee(int id)
        {
            var allocations = await _allocationService.GetByEmployeeAsync(id);
            return Ok(allocations);
        }

        /// <summary>
        /// Retrieves allocations for a specific project.
        /// </summary>
        [HttpGet("project/{id}")]
        public async Task<IActionResult> GetByProject(int id)
        {
            var allocations = await _allocationService.GetByProjectAsync(id);
            return Ok(allocations);
        }

        /// <summary>
        /// Creates a new allocation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Allocate([FromBody] CreateAllocationDto dto)
        {
            try
            {
                var allocation = await _allocationService.AllocateAsync(dto);
                return Ok(allocation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Ends a specific allocation.
        /// </summary>
        [HttpPut("{id}/end")]
        public async Task<IActionResult> End(int id)
        {
            await _allocationService.EndAllocationAsync(id);
            return Ok();
        }
    }
}
