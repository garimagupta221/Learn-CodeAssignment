using PrmServer.DTOs;
using PrmServer.Entities;

namespace PrmServer.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<List<Employee>> GetAvailableAsync();
        Task<float> GetUtilizationAsync(int employeeId);
        Task<Employee> CreateAsync(CreateEmployeeDto dto);
        Task<Employee> UpdateAsync(int id, UpdateEmployeeDto dto);
        Task DeactivateAsync(int id);
        Task ReactivateAsync(int id);
        Task<List<EmployeeSkillDto>> GetEmployeeSkillsAsync(int employeeId);
        Task AssignSkillAsync(int employeeId, AssignSkillDto dto);
        Task UpdateSkillAsync(int employeeId, int skillId, string proficiency);
        Task RemoveSkillAsync(int employeeId, int skillId);
        Task AssignManagerAsync(AssignManagerDto dto);
        Task<List<Employee>> GetByManagerUserIdAsync(int managerUserId);
    }
}
