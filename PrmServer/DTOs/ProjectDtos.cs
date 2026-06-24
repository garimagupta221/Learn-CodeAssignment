namespace PrmServer.DTOs
{
    public class ProjectSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Health { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public int TotalStoryPoints { get; set; }
        public int CompletedStoryPoints { get; set; }
    }

    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public int TotalStoryPoints { get; set; }
    }

    public class UpdateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public int TotalStoryPoints { get; set; }
    }

    public class AddMilestoneDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int StoryPoints { get; set; }
    }

    public class UpdateMilestoneDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
