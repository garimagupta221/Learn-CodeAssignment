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
            var timesheet = await _timesheetService.SubmitAsync(dto);
            return Ok(timesheet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTimesheetDto dto)
        {
            var timesheet = await _timesheetService.UpdateAsync(id, dto);
            return Ok(timesheet);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _timesheetService.ApproveAsync(id, 0);
            return Ok();
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectTimesheetDto dto)
        {
            await _timesheetService.RejectAsync(id, dto.Reason, 0);
            return Ok();
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
