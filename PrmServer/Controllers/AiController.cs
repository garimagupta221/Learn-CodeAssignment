using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;
using System.Security.Claims;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize(Roles = "Manager,Admin")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("skill-match")]
        public async Task<IActionResult> SkillMatch([FromBody] SkillMatchRequestDto dto)
        {
            var managerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!);
            var result = await _aiService.GetSkillMatchAsync(dto.Requirement, dto.ProjectId, dto.MaxHours, managerUserId);
            return Ok(new AiResponseDto { Result = result });
        }

        [HttpGet("risk-summary/{projectId}")]
        public async Task<IActionResult> RiskSummary(int projectId)
        {
            var result = await _aiService.GetProjectRiskSummaryAsync(projectId);
            return Ok(new AiResponseDto { Result = result });
        }
    }
}
