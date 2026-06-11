namespace PrmServer.Services.Interfaces
{
    public interface IAiService
    {
        Task<string> GetSkillMatchAsync(string requirement, int projectId, int? maxHours, int managerUserId);
        Task<string> GetProjectRiskSummaryAsync(int projectId);
    }
}
