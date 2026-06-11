using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/timesheets")]
    public class TimesheetController : ControllerBase
    {
        private readonly ITimesheetService _timesheetService;

        public TimesheetController(ITimesheetService timesheetService)
        {
            _timesheetService = timesheetService;
        }

        [HttpGet("employee/{id}")]
        public async Task<IActionResult> GetByEmployee(int id)
        {
            var timesheets = await _timesheetService.GetByEmployeeAsync(id);
            return Ok(timesheets);
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitTimesheetDto dto)
        {
            try
            {
                var timesheet = await _timesheetService.SubmitAsync(dto);
                return Ok(timesheet);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("missing-current-week")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetMissingCurrentWeek()
        {
            var employees = await _timesheetService.GetEmployeesMissingCurrentWeekAsync();
            return Ok(employees);
        }
    }
}
