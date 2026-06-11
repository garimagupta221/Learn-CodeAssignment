using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly PrmDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IAllocationService _allocationService;

        public EmployeeService(
            PrmDbContext context,
            IUserRepository userRepository,
            ISkillRepository skillRepository,
            IAllocationService allocationService)
        {
            _context = context;
            _userRepository = userRepository;
            _skillRepository = skillRepository;
            _allocationService = allocationService;
        }

        private Employee MapToEmployee(User user)
        {
            return new Employee
            {
                Id = user.Id,
                UserId = user.Id,
                ManagerUserId = user.ManagerId,
                FullName = user.FullName,
                Email = user.Email,
                Department = user.Department ?? string.Empty,
                Designation = user.Designation ?? string.Empty,
                Status = user.Status?.Status ?? "BENCH",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                User = user
            };
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Status)
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                .ToListAsync();

            return users.Select(MapToEmployee).ToList();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Status)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null || !user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                return null;

            return MapToEmployee(user);
        }

        public async Task<List<Employee>> GetAvailableAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Status)
                .Where(u => u.IsActive && u.Status.Status == "BENCH" && u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                .ToListAsync();

            return users.Select(MapToEmployee).ToList();
        }

        public async Task<float> GetUtilizationAsync(int employeeId)
        {
            var allocations = await _allocationService.GetByEmployeeAsync(employeeId);
            return allocations
                .Where(a => a.IsActive && a.EndDate >= DateTime.UtcNow.Date)
                .Sum(a => (float)a.UtilizationPct);
        }

        public async Task<Employee> CreateAsync(CreateEmployeeDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId)
                ?? throw new KeyNotFoundException($"User {dto.UserId} not found.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Department = dto.Department;
            user.Designation = dto.Designation;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Ensure they have the "Engineer" role in the db
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Engineer");
            if (role == null)
            {
                role = new Role { RoleName = "Engineer" };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
            }

            var hasRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
            if (!hasRole)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                await _context.SaveChangesAsync();
            }

            // Ensure they have status
            var status = await _context.UserStatuses.FirstOrDefaultAsync(us => us.UserId == user.Id);
            if (status == null)
            {
                status = new UserStatus
                {
                    UserId = user.Id,
                    Status = "BENCH",
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserStatuses.Add(status);
            }
            await _context.SaveChangesAsync();

            // Refresh user from DB to include loaded status/roles
            var refreshedUser = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Status)
                .FirstOrDefaultAsync(u => u.Id == user.Id)
                ?? throw new InvalidOperationException("Failed to reload created user.");

            return MapToEmployee(refreshedUser);
        }

        public async Task<Employee> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee {id} not found.");

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee {id} not found.");

            user.FullName = dto.FullName;
            user.Department = dto.Department;
            user.Designation = dto.Designation;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapToEmployee(user);
        }

        public async Task DeactivateAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee {id} not found.");

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee {id} not found.");

            // Deactivation cascade: end all active allocations before deactivating
            await _allocationService.EndAllActiveAllocationsAsync(id);

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        public async Task ReactivateAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee {id} not found.");

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee {id} not found.");

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Re-set status to BENCH
            var status = await _context.UserStatuses.FirstOrDefaultAsync(us => us.UserId == id);
            if (status != null)
            {
                status.Status = "BENCH";
                status.UpdatedAt = DateTime.UtcNow;
                _context.UserStatuses.Update(status);
            }
            else
            {
                _context.UserStatuses.Add(new UserStatus
                {
                    UserId = id,
                    Status = "BENCH",
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<EmployeeSkillDto>> GetEmployeeSkillsAsync(int employeeId)
        {
            var user = await _userRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee {employeeId} not found.");

            var skills = await _context.UserSkills
                .Include(us => us.Skill)
                .Where(us => us.UserId == employeeId)
                .ToListAsync();

            return skills.Select(es => new EmployeeSkillDto
            {
                SkillId = es.SkillId,
                SkillName = es.Skill.Name,
                Category = es.Skill.Category,
                Proficiency = es.Proficiency
            }).ToList();
        }

        public async Task AssignSkillAsync(int employeeId, AssignSkillDto dto)
        {
            var user = await _userRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee {employeeId} not found.");

            var skill = await _skillRepository.GetByNameAsync(dto.SkillName);
            if (skill == null)
            {
                skill = await _skillRepository.AddAsync(new Skill
                {
                    Name = dto.SkillName,
                    Category = dto.Category,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var existing = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == employeeId && us.SkillId == skill.Id);
            if (existing != null)
                throw new InvalidOperationException("This skill is already assigned to the employee.");

            var userSkill = new UserSkill
            {
                UserId = employeeId,
                SkillId = skill.Id,
                Proficiency = dto.Proficiency,
                AssignedAt = DateTime.UtcNow
            };

            _context.UserSkills.Add(userSkill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSkillAsync(int employeeId, int skillId, string proficiency)
        {
            var user = await _userRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee {employeeId} not found.");

            var userSkill = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == employeeId && us.SkillId == skillId)
                ?? throw new KeyNotFoundException($"Skill {skillId} not found for employee {employeeId}.");

            userSkill.Proficiency = proficiency;
            _context.UserSkills.Update(userSkill);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSkillAsync(int employeeId, int skillId)
        {
            var user = await _userRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee {employeeId} not found.");

            if (!user.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee {employeeId} not found.");

            var userSkill = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == employeeId && us.SkillId == skillId)
                ?? throw new KeyNotFoundException($"Skill {skillId} not found for employee {employeeId}.");

            _context.UserSkills.Remove(userSkill);
            await _context.SaveChangesAsync();
        }

        public async Task AssignManagerAsync(AssignManagerDto dto)
        {
            var employee = await _userRepository.GetByIdAsync(dto.EmployeeUserId)
                ?? throw new KeyNotFoundException($"Employee with User ID {dto.EmployeeUserId} not found.");

            if (!employee.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                throw new KeyNotFoundException($"Employee with User ID {dto.EmployeeUserId} not found.");

            var manager = await _userRepository.GetByIdAsync(dto.ManagerUserId)
                ?? throw new KeyNotFoundException($"User {dto.ManagerUserId} not found.");

            var isManager = manager.UserRoles.Any(ur => ur.Role.RoleName == "Manager") || manager.Designation == "Manager";
            if (!isManager)
                throw new InvalidOperationException($"User {dto.ManagerUserId} is not a Manager.");

            employee.ManagerId = dto.ManagerUserId;
            employee.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(employee);
        }

        public async Task<List<Employee>> GetByManagerUserIdAsync(int managerUserId)
        {
            var engineers = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Status)
                .Where(u => u.ManagerId == managerUserId && u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                .ToListAsync();

            return engineers.Select(MapToEmployee).ToList();
        }
    }
}
