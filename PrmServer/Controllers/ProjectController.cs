using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectService.GetAllAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound($"Project with id {id} not found.");
            return Ok(project);
        }

        [HttpGet("manager/{id}")]
        public async Task<IActionResult> GetByManager(int id)
        {
            var projects = await _projectService.GetByManagerAsync(id);
            return Ok(projects);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            try
            {
                var project = await _projectService.CreateAsync(dto);
                return Ok(project);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var project = await _projectService.UpdateAsync(id, dto);
                return Ok(project);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/health")]
        public async Task<IActionResult> GetHealth(int id)
        {
            var health = await _projectService.GetHealthAsync(id);
            return Ok(new { health });
        }

        [HttpPost("{id}/milestones")]
        public async Task<IActionResult> AddMilestone(int id, [FromBody] AddMilestoneDto dto)
        {
            try
            {
                var milestone = await _projectService.AddMilestoneAsync(id, dto);
                return Ok(milestone);
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
    }
}
