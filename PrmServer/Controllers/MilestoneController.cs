using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/milestones")]
    public class MilestoneController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public MilestoneController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(int projectId)
        {
            var milestones = await _projectService.GetMilestonesAsync(projectId);
            return Ok(milestones);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMilestoneDto dto)
        {
            var milestone = await _projectService.UpdateMilestoneAsync(id, dto);
            return Ok(milestone);
        }
    }
}
