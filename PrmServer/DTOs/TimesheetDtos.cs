namespace PrmServer.DTOs
{
    public class SubmitTimesheetDto
    {
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public DateTime WeekStart { get; set; }
        public float HoursLogged { get; set; }
        public List<int> TagIds { get; set; } = new();
    }

    public class UpdateTimesheetDto
    {
        public float HoursLogged { get; set; }
        public List<int> TagIds { get; set; } = new();
    }

    public class RejectTimesheetDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
