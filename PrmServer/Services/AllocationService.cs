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
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly PrmDbContext _context;

        public AllocationService(
            IAllocationRepository allocationRepository,
            PrmDbContext context)
        {
            _allocationRepository = allocationRepository;
            _context = context;
        }

        public Task<List<Allocation>> GetAllAsync()
        {
            return _allocationRepository.GetAllAsync();
        }

        public Task<List<Allocation>> GetByEmployeeAsync(int employeeId)
        {
            return _allocationRepository.GetByUserIdAsync(employeeId);
        }

        public Task<List<Allocation>> GetByProjectAsync(int projectId)
        {
            return _allocationRepository.GetByProjectIdAsync(projectId);
        }

        public async Task<Allocation> AllocateAsync(CreateAllocationDto dto)
        {
            if (dto.UtilizationPct < 1 || dto.UtilizationPct > 100)
                throw new ArgumentException("Utilization must be between 1 and 100.");

            if (dto.StartDate >= dto.EndDate)
                throw new ArgumentException("Start date must be before end date.");

            var overAllocated = await IsOverAllocatedAsync(
                dto.EmployeeId, dto.UtilizationPct, dto.StartDate, dto.EndDate);

            if (overAllocated)
                throw new InvalidOperationException(
                    "Allocation would exceed 100% utilization for the employee in the given date range.");

            var allocation = new Allocation
            {
                UserId = dto.EmployeeId,
                ProjectId = dto.ProjectId,
                AllocatedBy = dto.AllocatedBy,
                UtilizationPct = dto.UtilizationPct,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _allocationRepository.AddAsync(allocation);

            await UpdateEmployeeStatusAsync(dto.EmployeeId);

            return created;
        }

        public async Task<bool> IsOverAllocatedAsync(
            int employeeId, int utilizationPct, DateTime startDate, DateTime endDate)
        {
            var existing = await _allocationRepository.GetByUserIdAsync(employeeId);

            var overlappingTotal = existing
                .Where(a => a.IsActive && a.StartDate < endDate && a.EndDate > startDate)
                .Sum(a => a.UtilizationPct);

            return overlappingTotal + utilizationPct > 100;
        }

        public async Task EndAllocationAsync(int allocationId)
        {
            var allocation = await _allocationRepository.GetByIdAsync(allocationId)
                ?? throw new KeyNotFoundException($"Allocation {allocationId} not found.");

            allocation.IsActive = false;
            allocation.EndDate = DateTime.UtcNow.Date;
            allocation.UpdatedAt = DateTime.UtcNow;

            await _allocationRepository.UpdateAsync(allocation);

            await UpdateEmployeeStatusAsync(allocation.UserId);
        }

        public async Task EndAllActiveAllocationsAsync(int employeeId)
        {
            var allocations = await _allocationRepository.GetByUserIdAsync(employeeId);
            var active = allocations.Where(a => a.IsActive).ToList();

            foreach (var allocation in active)
            {
                allocation.IsActive = false;
                allocation.EndDate = DateTime.UtcNow.Date;
                allocation.UpdatedAt = DateTime.UtcNow;
                await _allocationRepository.UpdateAsync(allocation);
            }
        }

        public async Task RecomputeAllEmployeeStatusesAsync()
        {
            var activeEngineers = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive && u.UserRoles.Any(ur => ur.Role.RoleName == "Engineer"))
                .ToListAsync();

            foreach (var engineer in activeEngineers)
            {
                await UpdateEmployeeStatusAsync(engineer.Id);
            }
        }

        private async Task UpdateEmployeeStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return;

            var allocations = await _allocationRepository.GetByUserIdAsync(userId);
            var hasActiveAllocation = allocations.Any(a => a.IsActive && a.EndDate >= DateTime.UtcNow.Date);

            var newStatusValue = hasActiveAllocation ? "ALLOCATED" : "BENCH";

            var userStatus = await _context.UserStatuses.FirstOrDefaultAsync(us => us.UserId == userId);
            if (userStatus == null)
            {
                userStatus = new UserStatus
                {
                    UserId = userId,
                    Status = newStatusValue,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserStatuses.Add(userStatus);
            }
            else
            {
                userStatus.Status = newStatusValue;
                userStatus.UpdatedAt = DateTime.UtcNow;
                _context.UserStatuses.Update(userStatus);
            }
            await _context.SaveChangesAsync();
        }
    }
}
