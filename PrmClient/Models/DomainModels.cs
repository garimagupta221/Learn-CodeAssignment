namespace PrmClient.Models
{
    public class EmployeeModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateEmployeeRequest
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }

    public class UpdateEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }

    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsTemporaryPassword { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProjectModel
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

    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public int TotalStoryPoints { get; set; }
    }

    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public int TotalStoryPoints { get; set; }
    }

    public class AllocationModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int UtilizationPct { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class MilestoneModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StoryPoints { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class AddMilestoneRequest
    {
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int StoryPoints { get; set; }
    }

    public class UpdateMilestoneRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class CreateAllocationRequest
    {
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public int AllocatedBy { get; set; }
        public int UtilizationPct { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class TimesheetModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public float HoursLogged { get; set; }
        public string Status { get; set; } = string.Empty;   // "SUBMITTED" | "MISSED"
        public List<TimesheetTagModel> TimesheetTags { get; set; } = new();
    }

    public class TimesheetTagModel
    {
        public ActivityTagModel ActivityTag { get; set; } = new();
    }

    public class ActivityTagModel
    {
        public int Id { get; set; }
        public string TagName { get; set; } = string.Empty;
    }

    public class SubmitTimesheetRequest
    {
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public DateTime WeekStart { get; set; }
        public float HoursLogged { get; set; }
        public List<int> TagIds { get; set; } = new();
    }

    public class SetConfigRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class AssignSkillRequest
    {
        public string SkillName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
    }

    public class EmployeeSkillModel
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
    }

    public class UpdateSkillDto
    {
        public string Proficiency { get; set; } = string.Empty;
    }


    public class MissingTimesheetEmployee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class SkillMatchRequestDto
    {
        public string Requirement { get; set; } = string.Empty;
        public int? ProjectId { get; set; }
        public int? MaxHours { get; set; }
    }

    public class AiResponseDto
    {
        public string Result { get; set; } = string.Empty;
    }

    public class SkillMatchRecommendationModel
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SkillsMatch { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;
        public string RecentActivity { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class SkillMatchResponseDto
    {
        public List<SkillMatchRecommendationModel> Recommendations { get; set; } = new();
        public string Provider { get; set; } = string.Empty;
    }

    public class AssignManagerRequest
    {
        public int EmployeeUserId { get; set; }
        public int ManagerUserId { get; set; }
    }
}
