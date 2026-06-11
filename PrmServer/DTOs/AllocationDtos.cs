namespace PrmServer.DTOs
{
    public class CreateAllocationDto
    {
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public int AllocatedBy { get; set; }
        public int UtilizationPct { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
