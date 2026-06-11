using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IUserRepository _userRepository;

        // Number of days before due date that a pending milestone triggers AMBER health
        private const int AmberWarningDays = 7;

        public ProjectService(
            IProjectRepository projectRepository,
            IMilestoneRepository milestoneRepository,
            IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _milestoneRepository = milestoneRepository;
            _userRepository = userRepository;
        }

        public async Task<List<ProjectSummaryDto>> GetAllAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            return await ToSummariesAsync(projects);
        }

        public Task<Project?> GetByIdAsync(int id)
        {
            return _projectRepository.GetByIdAsync(id);
        }

        public async Task<List<ProjectSummaryDto>> GetByManagerAsync(int managerId)
        {
            var projects = await _projectRepository.GetByManagerIdAsync(managerId);
            return await ToSummariesAsync(projects);
        }

        public async Task<Project> CreateAsync(CreateProjectDto dto)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new ArgumentException("Start date must be before end date.");

            var manager = await _userRepository.GetByIdAsync(dto.ManagerId)
                ?? throw new KeyNotFoundException($"No user found with ID {dto.ManagerId}.");

            if (!string.Equals(manager.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"User '{manager.Username}' (User ID: {dto.ManagerId}) is not a Manager and cannot be assigned as a project manager.");

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ManagerId = manager.Id,   // store Employee.Id, not User.Id
                TotalStoryPoints = dto.TotalStoryPoints,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "PLANNED" : dto.Status,
                Health = "GREEN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _projectRepository.AddAsync(project);
        }

        public async Task<Project> UpdateAsync(int id, UpdateProjectDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Project {id} not found.");

            var manager = await _userRepository.GetByIdAsync(dto.ManagerId)
                ?? throw new KeyNotFoundException($"No user found with ID {dto.ManagerId}.");

            if (!string.Equals(manager.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"User '{manager.Username}' (User ID: {dto.ManagerId}) is not a Manager.");

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.Status = dto.Status;
            project.ManagerId = manager.Id;   // store Employee.Id, not User.Id
            project.TotalStoryPoints = dto.TotalStoryPoints;
            project.UpdatedAt = DateTime.UtcNow;

            return await _projectRepository.UpdateAsync(project);
        }

        public async Task<string> GetHealthAsync(int projectId)
        {
            var milestones = await _milestoneRepository.GetByProjectIdAsync(projectId);

            if (!milestones.Any())
                return "GREEN";

            var today = DateTime.UtcNow.Date;

            var hasOverdue = milestones.Any(m =>
                m.Status != "COMPLETED" && m.DueDate.Date < today);

            if (hasOverdue)
                return "RED";

            var hasApproachingDeadline = milestones.Any(m =>
                m.Status == "PENDING" && m.DueDate.Date <= today.AddDays(AmberWarningDays));

            return hasApproachingDeadline ? "AMBER" : "GREEN";
        }

        public async Task<Milestone> AddMilestoneAsync(int projectId, AddMilestoneDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            // Guard: sum of existing milestone SPs + new SP must not exceed project TotalStoryPoints
            var existingMilestones = await _milestoneRepository.GetByProjectIdAsync(projectId);
            int usedSP = existingMilestones.Sum(m => m.StoryPoints);
            if (usedSP + dto.StoryPoints > project.TotalStoryPoints)
            {
                int remaining = project.TotalStoryPoints - usedSP;
                throw new InvalidOperationException(
                    $"Milestone points cannot exceed the project's total remaining points. " +
                    $"(Remaining: {remaining} SP, Requested: {dto.StoryPoints} SP)");
            }

            var milestone = new Milestone
            {
                ProjectId = projectId,
                Title = dto.Title,
                DueDate = dto.DueDate,
                StoryPoints = dto.StoryPoints,
                Status = "PENDING",
                UpdatedAt = DateTime.UtcNow
            };

            return await _milestoneRepository.AddAsync(milestone);
        }

        public async Task<Milestone> UpdateMilestoneAsync(int milestoneId, UpdateMilestoneDto dto)
        {
            var milestone = await _milestoneRepository.GetByIdAsync(milestoneId)
                ?? throw new KeyNotFoundException($"Milestone {milestoneId} not found.");

            milestone.Status = dto.Status;
            milestone.UpdatedAt = DateTime.UtcNow;

            return await _milestoneRepository.UpdateAsync(milestone);
        }

        public Task<List<Milestone>> GetMilestonesAsync(int projectId)
        {
            return _milestoneRepository.GetByProjectIdAsync(projectId);
        }

        public async Task CompleteMilestoneAsync(int milestoneId)
        {
            var milestone = await _milestoneRepository.GetByIdAsync(milestoneId)
                ?? throw new KeyNotFoundException($"Milestone {milestoneId} not found.");

            milestone.Status = "COMPLETED";
            milestone.CompletedAt = DateTime.UtcNow;
            milestone.UpdatedAt = DateTime.UtcNow;

            await _milestoneRepository.UpdateAsync(milestone);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private async Task<List<ProjectSummaryDto>> ToSummariesAsync(List<Project> projects)
        {
            var result = new List<ProjectSummaryDto>();
            foreach (var p in projects)
            {
                var milestones = await _milestoneRepository.GetByProjectIdAsync(p.Id);
                var completedSP = milestones
                    .Where(m => m.Status == "DONE")
                    .Sum(m => m.StoryPoints);

                result.Add(new ProjectSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    Health = p.Health,
                    ManagerId = p.ManagerId,
                    ManagerName = p.Manager?.FullName ?? string.Empty,
                    TotalStoryPoints = p.TotalStoryPoints,
                    CompletedStoryPoints = completedSP
                });
            }
            return result;
        }
    }
}
