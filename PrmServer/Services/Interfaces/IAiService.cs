using PrmServer.DTOs;

namespace PrmServer.Services.Interfaces
{
    public interface IAiService
    {
        Task<SkillMatchResult> GetSkillMatchAsync(string requirement, int? projectId, int? maxHours, int managerUserId);
        Task<AiResult> GetProjectRiskSummaryAsync(int projectId, int managerUserId);
        /// <summary>
        /// Staffs an entire project team in one pass — returns the best 100%-available
        /// bench employee per role, deduplicates across roles, and explains any gaps.
        /// </summary>
        Task<TeamBuilderResult> BuildTeamAsync(TeamBuilderRequestDto request, int managerUserId);
    }
}

