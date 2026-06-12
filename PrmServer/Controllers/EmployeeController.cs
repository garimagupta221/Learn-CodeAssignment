using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Retrieves all employees.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        /// <summary>
        /// Retrieves an employee by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee is null)
                return NotFound($"Employee with id {id} not found.");
            return Ok(employee);
        }

        /// <summary>
        /// Retrieves available employees.
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var employees = await _employeeService.GetAvailableAsync();
            return Ok(employees);
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var employee = await _employeeService.CreateAsync(dto);
            return Ok(employee);
        }

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.UpdateAsync(id, dto);
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivates an employee.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _employeeService.DeactivateAsync(id);
            return Ok();
        }

        /// <summary>
        /// Retrieves skills for a specific employee.
        /// </summary>
        [HttpGet("{id}/skills")]
        public async Task<IActionResult> GetSkills(int id)
        {
            var skills = await _employeeService.GetEmployeeSkillsAsync(id);
            return Ok(skills);
        }

        /// <summary>
        /// Assigns a skill to an employee.
        /// </summary>
        [HttpPost("{id}/skills")]
        public async Task<IActionResult> AssignSkill(int id, [FromBody] AssignSkillDto dto)
        {
            await _employeeService.AssignSkillAsync(id, dto);
            return Ok();
        }

        /// <summary>
        /// Updates a skill's proficiency for an employee.
        /// </summary>
        [HttpPut("{id}/skills/{skillId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSkill(int id, int skillId, [FromBody] UpdateSkillDto dto)
        {
            await _employeeService.UpdateSkillAsync(id, skillId, dto.Proficiency);
            return Ok();
        }

        /// <summary>
        /// Removes a skill from an employee.
        /// </summary>
        [HttpDelete("{id}/skills/{skillId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSkill(int id, int skillId)
        {
            await _employeeService.RemoveSkillAsync(id, skillId);
            return Ok();
        }

        /// <summary>
        /// Assigns a manager to an employee.
        /// </summary>
        [HttpPut("assign-manager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignManager([FromBody] AssignManagerDto dto)
        {
            try
            {
                await _employeeService.AssignManagerAsync(dto);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves employees by manager user ID.
        /// </summary>
        [HttpGet("by-manager/{managerUserId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetByManager(int managerUserId)
        {
            var employees = await _employeeService.GetByManagerUserIdAsync(managerUserId);
            return Ok(employees);
        }
    }
}
