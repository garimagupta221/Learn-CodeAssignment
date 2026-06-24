namespace PrmServer.DTOs
{
    public class CreateEmployeeDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }

    public class UpdateEmployeeDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }

    public class AssignSkillDto
    {
        public string SkillName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
    }

    public class EmployeeSkillDto
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

    public class AssignManagerDto
    {
        public int EmployeeUserId { get; set; }
        public int ManagerUserId { get; set; }
    }
}
