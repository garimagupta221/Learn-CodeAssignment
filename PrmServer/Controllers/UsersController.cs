using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly PrmDbContext _context;
        private readonly IAllocationService _allocationService;

        public UsersController(
            IUserRepository userRepository,
            PrmDbContext context,
            IAllocationService allocationService)
        {
            _userRepository = userRepository;
            _context = context;
            _allocationService = allocationService;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllAsync();
            var dtos = users.Select(u => new UserDto
            {
                Id                  = u.Id,
                Username            = u.Username,
                Email               = u.Email,
                Role                = u.Role,
                IsActive            = u.IsActive,
                IsTemporaryPassword = u.IsTemporaryPassword,
                CreatedAt           = u.CreatedAt
            }).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Retrieves a user by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return NotFound($"User {id} not found.");

            return Ok(new UserDto
            {
                Id                  = user.Id,
                Username            = user.Username,
                Email               = user.Email,
                Role                = user.Role,
                IsActive            = user.IsActive,
                IsTemporaryPassword = user.IsTemporaryPassword,
                CreatedAt           = user.CreatedAt
            });
        }

        /// <summary>
        /// Deactivates a user.
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return NotFound($"User {id} not found.");

            // Self-deactivation check
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int loggedInId) && loggedInId == id)
            {
                return BadRequest(new { message = "Admin cannot deactivate itself." });
            }

            // End active allocations if they are an Engineer
            var isEngineer = user.UserRoles?.Any(ur => ur.Role.RoleName == "Engineer") ?? false;
            if (isEngineer)
            {
                await _allocationService.EndAllActiveAllocationsAsync(id);
            }

            user.IsActive    = false;
            user.UpdatedAt   = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Update user status to DEACTIVATED
            var statusObj = await _context.UserStatuses.FirstOrDefaultAsync(us => us.UserId == id);
            if (statusObj != null)
            {
                statusObj.Status = "DEACTIVATED";
                statusObj.UpdatedAt = DateTime.UtcNow;
                _context.UserStatuses.Update(statusObj);
            }
            else
            {
                _context.UserStatuses.Add(new UserStatus
                {
                    UserId = id,
                    Status = "DEACTIVATED",
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deactivated successfully." });
        }

        /// <summary>
        /// Activates a user.
        /// </summary>
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return NotFound($"User {id} not found.");

            if (user.IsActive)
                return BadRequest(new { message = "User is already active." });

            user.IsActive  = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Set user status to BENCH on reactivation
            var statusObj = await _context.UserStatuses.FirstOrDefaultAsync(us => us.UserId == id);
            if (statusObj != null)
            {
                statusObj.Status = "BENCH";
                statusObj.UpdatedAt = DateTime.UtcNow;
                _context.UserStatuses.Update(statusObj);
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

            return Ok(new { message = "User activated successfully." });
        }

        /// <summary>
        /// Forces a password reset for a user on their next login.
        /// </summary>
        [HttpPut("{id}/force-password-reset")]
        public async Task<IActionResult> ForcePasswordReset(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return NotFound($"User {id} not found.");

            user.IsTemporaryPassword = true;
            user.UpdatedAt           = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return Ok(new { });
        }

        /// <summary>
        /// Looks up a user by ID or username.
        /// </summary>
        [HttpGet("lookup/{identifier}")]
        public async Task<IActionResult> Lookup(string identifier)
        {
            User? user = null;
            if (int.TryParse(identifier, out int id))
            {
                user = await _userRepository.GetByIdAsync(id);
            }

            if (user == null)
            {
                user = await _userRepository.GetByUsernameAsync(identifier);
            }

            if (user is null)
                return NotFound($"User '{identifier}' not found.");

            return Ok(new UserDto
            {
                Id                  = user.Id,
                Username            = user.Username,
                Email               = user.Email,
                Role                = user.Role,
                IsActive            = user.IsActive,
                IsTemporaryPassword = user.IsTemporaryPassword,
                CreatedAt           = user.CreatedAt
            });
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return NotFound($"User {id} not found.");

            user.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.IsTemporaryPassword = true;
            user.UpdatedAt           = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return Ok(new { });
        }
    }
}
