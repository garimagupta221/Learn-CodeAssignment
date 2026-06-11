using PrmServer.DTOs;
using PrmServer.Entities;

namespace PrmServer.Services.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectSummaryDto>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task<List<ProjectSummaryDto>> GetByManagerAsync(int managerId);
        Task<Project> CreateAsync(CreateProjectDto dto);
        Task<Project> UpdateAsync(int id, UpdateProjectDto dto);
        Task<string> GetHealthAsync(int projectId);
        Task<Milestone> AddMilestoneAsync(int projectId, AddMilestoneDto dto);
        Task<Milestone> UpdateMilestoneAsync(int milestoneId, UpdateMilestoneDto dto);
        Task<List<Milestone>> GetMilestonesAsync(int projectId);
        Task CompleteMilestoneAsync(int milestoneId);
    }
}
