using PrmServer.DTOs;
using PrmServer.Entities;

namespace PrmServer.Services.Interfaces
{
    public interface IAllocationService
    {
        Task<List<Allocation>> GetAllAsync();
        Task<List<Allocation>> GetByEmployeeAsync(int employeeId);
        Task<List<Allocation>> GetByProjectAsync(int projectId);
        Task<Allocation> AllocateAsync(CreateAllocationDto dto);
        Task<bool> IsOverAllocatedAsync(int employeeId, int utilizationPct, DateTime startDate, DateTime endDate);
        Task EndAllocationAsync(int allocationId);
        Task EndAllActiveAllocationsAsync(int employeeId);
        Task RecomputeAllEmployeeStatusesAsync();
    }
}
