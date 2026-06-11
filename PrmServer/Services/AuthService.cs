using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PrmServer.DTOs;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PrmDbContext _context;
        private readonly IConfiguration _configuration;

        // In-memory revoked token store (sufficient for single-instance deployments)
        private static readonly HashSet<string> RevokedTokens = new();

        public AuthService(IUserRepository userRepository, PrmDbContext context, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username)
                ?? throw new UnauthorizedAccessException("Invalid username or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid username or password.");

            return new LoginResponseDto
            {
                Token               = GenerateToken(user),
                UserId              = user.Id,
                FullName            = user.FullName,
                Role                = user.Role,
                IsTemporaryPassword = user.IsTemporaryPassword
            };
        }

        public async Task<UserDto> SignUpAsync(SignUpDto dto)
        {
            // Proactive duplicate validation — return clean error before touching the DB
            var existingByUsername = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingByUsername != null)
                throw new InvalidOperationException("Username or Email already exists.");

            var existingByEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingByEmail != null)
                throw new InvalidOperationException("Username or Email already exists.");

            // Wrap User + Role & Status creation in a single atomic transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    FullName            = string.IsNullOrWhiteSpace(dto.FullName) ? dto.Username : dto.FullName,
                    Email               = dto.Email,
                    Username            = dto.Username,
                    PasswordHash        = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Designation         = dto.Role,
                    Department          = string.Empty,
                    IsActive            = true,
                    IsTemporaryPassword = true,
                    CreatedAt           = DateTime.UtcNow,
                    UpdatedAt           = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Fetch or create Role
                var roleObj = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
                if (roleObj == null)
                {
                    roleObj = new Role { RoleName = dto.Role };
                    _context.Roles.Add(roleObj);
                    await _context.SaveChangesAsync();
                }

                // Create UserRole link
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleObj.Id
                };
                _context.UserRoles.Add(userRole);

                // Auto-create UserStatus if they are an Engineer
                if (string.Equals(dto.Role, "Engineer", StringComparison.OrdinalIgnoreCase))
                {
                    var status = new UserStatus
                    {
                        UserId = user.Id,
                        Status = "BENCH",
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserStatuses.Add(status);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload user to ensure roles are populated in returned DTO
                var refreshed = await _userRepository.GetByIdAsync(user.Id)
                    ?? throw new InvalidOperationException("Failed to reload registered user.");

                return new UserDto
                {
                    Id                  = refreshed.Id,
                    Username            = refreshed.Username,
                    Email               = refreshed.Email,
                    Role                = refreshed.Role,
                    IsActive            = refreshed.IsActive,
                    IsTemporaryPassword = refreshed.IsTemporaryPassword,
                    CreatedAt           = refreshed.CreatedAt
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task LogoutAsync(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
                RevokedTokens.Add(token);

            return Task.CompletedTask;
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId)
                ?? throw new KeyNotFoundException($"User {dto.UserId} not found.");

            user.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsTemporaryPassword = false;
            user.UpdatedAt           = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        private string GenerateToken(User user)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiryHours = int.Parse(jwtSection["ExpiryHours"] ?? "8");

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
