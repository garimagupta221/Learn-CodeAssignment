using Microsoft.AspNetCore.Mvc;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

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

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                return NotFound($"User {id} not found.");

            user.IsActive    = false;
            user.UpdatedAt   = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            return Ok(new { });
        }

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
            return Ok(new { });
        }

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
