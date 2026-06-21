using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Services.Interfaces;
using System.Security.Claims;

namespace PrmServer.Controllers
{
    /// <summary>
    /// AI-powered features: Skill Matcher and Project Risk Summary.
    /// Accessible by Managers and Admins. The active LLM provider is configurable
    /// by an Admin via POST /api/system-config (key: ActiveAiProvider).
    /// </summary>
    [ApiController]
    [Route("api/ai")]
    [Authorize(Roles = "Manager,Admin")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly ILogger<AiController> _logger;

        public AiController(IAiService aiService, ILogger<AiController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Skill Matcher — finds the best-matched employees for a natural-language requirement.
        /// The server gathers employee skill and timesheet data, passes it to the configured
        /// LLM, and returns a ranked suggestion list with justifications.
        /// </summary>
        /// <remarks>
        /// <para><b>Required body fields:</b></para>
        /// <list type="bullet">
        ///   <item><description><b>requirement</b> — free-text description of what you need (e.g. "React developer with 2+ years experience")</description></item>
        ///   <item><description><b>projectId</b> — used to include prior project hours in the employee profile sent to the LLM</description></item>
        ///   <item><description><b>maxHours</b> (optional) — if set, the LLM is told to prefer employees under this weekly-hour cap</description></item>
        /// </list>
        /// </remarks>
        [HttpPost("skill-match")]
        [ProducesResponseType(typeof(SkillMatchResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> SkillMatch([FromBody] SkillMatchRequestDto dto)
        {
            var managerUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!);

            try
            {
                var aiResult = await _aiService.GetSkillMatchAsync(
                    dto.Requirement, dto.ProjectId, dto.MaxHours, managerUserId);

                return Ok(new SkillMatchResponseDto
                {
                    Recommendations = aiResult.Recommendations,
                    Provider        = aiResult.Provider
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Skill match failed for manager {ManagerId}", managerUserId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Risk Summary — returns a plain-English health and risk analysis for a project.
        /// The server collects milestone statuses and timesheet data, passes them to the
        /// configured LLM, and returns a structured risk assessment.
        /// If the LLM provider is unavailable, a system-generated fallback summary is
        /// returned (graceful degradation — never returns a 500).
        /// </summary>
        [HttpGet("risk-summary/{projectId:int}")]
        [ProducesResponseType(typeof(AiResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> RiskSummary(int projectId)
        {
            var managerUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!);

            try
            {
                var aiResult = await _aiService.GetProjectRiskSummaryAsync(projectId, managerUserId);
                return Ok(new AiResponseDto
                {
                    Result   = aiResult.Result,
                    Provider = aiResult.Provider
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
        }
        /// <summary>
        /// Team Builder — staffs an entire project team in one AI pass.
        /// Only 100%-bench employees (no active allocation today) are candidates.
        /// Managers see all engineers company-wide.
        /// Returns a per-role result: filled (with employee + reason) or a gap (NoSkill / Allocated).
        /// </summary>
        [HttpPost("team-builder")]
        [ProducesResponseType(typeof(TeamBuilderResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BuildTeam([FromBody] TeamBuilderRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ProjectName))
                return BadRequest(new { error = "ProjectName is required." });

            if (string.IsNullOrWhiteSpace(dto.TeamRequirement))
                return BadRequest(new { error = "Team requirement prompt is required." });

            var managerUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!);

            try
            {
                var result = await _aiService.BuildTeamAsync(dto, managerUserId);
                return Ok(new TeamBuilderResponseDto
                {
                    ProjectName = result.ProjectName,
                    Results     = result.Results,
                    Provider    = result.Provider
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Team Builder failed for manager {ManagerId}", managerUserId);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
