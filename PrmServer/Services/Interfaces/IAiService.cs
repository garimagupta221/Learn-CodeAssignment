using PrmServer.DTOs;

namespace PrmServer.Services.Interfaces
{
    public interface IAiService
    {
        Task<SkillMatchResult> GetSkillMatchAsync(string requirement, int? projectId, int? maxHours, int managerUserId);
        Task<AiResult> GetProjectRiskSummaryAsync(int projectId, int managerUserId);
    }
}

