using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrmServer.Entities;

namespace PrmServer.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(PrmDbContext context)
        {
            // Seed system roles if not present
            var defaultRoles = new[] { "Admin", "Manager", "Engineer" };
            var roleEntities = new Dictionary<string, Role>();
            foreach (var roleName in defaultRoles)
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                {
                    role = new Role { RoleName = roleName };
                    context.Roles.Add(role);
                }
                roleEntities[roleName] = role;
            }
            await context.SaveChangesAsync();

            if (!context.Users.Any())
            {
                var adminUser = new User
                {
                    Username = "admin",
                    FullName = "Administrator",
                    Email = "admin@prm.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                    Designation = "Admin",
                    Department = "Administration",
                    IsTemporaryPassword = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                // Link to Admin role
                var adminRole = roleEntities["Admin"];
                context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });

                await context.SaveChangesAsync();
            }

            // Seed the BRD activity tags if not already present
            if (!context.ActivityTags.Any())
            {
                var tags = new[]
                {
                    new ActivityTag { TagName = "Backend API Development",      Category = "Development", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Microservices / Architecture", Category = "Development", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Database Design & Queries",    Category = "Development", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "WebSocket / Real-time Features", Category = "Development", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Frontend Development",         Category = "Development", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Code Review / Mentoring",      Category = "Quality",     IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Bug Fixing",                   Category = "Quality",     IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "DevOps / Deployment",          Category = "Operations",  IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Testing & QA",                 Category = "Quality",     IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Documentation",                Category = "General",     IsActive = true, CreatedAt = DateTime.UtcNow },
                    new ActivityTag { TagName = "Other",                        Category = "General",     IsActive = true, CreatedAt = DateTime.UtcNow },
                };

                context.ActivityTags.AddRange(tags);
                await context.SaveChangesAsync();
            }

            // Seed default system configurations
            if (!context.SystemConfigs.Any())
            {
                var configs = new[]
                {
                    new SystemConfig { Key = "ActiveAiProvider", Value = "Gemini" },
                    new SystemConfig { Key = "AiApiKey",         Value = "" },
                    new SystemConfig { Key = "GemmaBaseUrl",     Value = "http://localhost:11434" },
                    new SystemConfig { Key = "GemmaModel",       Value = "gemma3:12b-it-q8_0" },
                    new SystemConfig { Key = "SchedulerInterval", Value = "4" },
                    new SystemConfig { Key = "MaxWeeklyHours",   Value = "40" }
                };

                context.SystemConfigs.AddRange(configs);
                await context.SaveChangesAsync();
            }
        }
    }
}
