using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PrmServer.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsTemporaryPassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User Manager { get; set; }
        public ICollection<User> DirectReports { get; set; }
        public UserStatus Status { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<UserSkill> UserSkills { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Allocation> Allocations { get; set; }
        public ICollection<Allocation> CreatedAllocations { get; set; }
        public ICollection<Timesheet> Timesheets { get; set; }
        public ICollection<Timesheet> ApprovedTimesheets { get; set; }
        public ICollection<Project> ManagedProjects { get; set; }

        // Backward compatibility helper
        [NotMapped]
        public string Role => UserRoles?.FirstOrDefault()?.Role?.RoleName ?? Designation ?? string.Empty;
    }

    // Non-db class for backward compatibility with Service/Controller layers
    public class Employee
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ManagerUserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public ICollection<UserSkill> UserSkills { get; set; }
        public ICollection<Allocation> Allocations { get; set; }
        public ICollection<Timesheet> Timesheets { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public User User { get; set; }
        public Role Role { get; set; }
    }

    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; }
    }

    public class UserSkill
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SkillId { get; set; }
        public string Proficiency { get; set; }
        public DateTime AssignedAt { get; set; }

        public User User { get; set; }
        public Skill Skill { get; set; }
    }

    public class UserStatus
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
    }

    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string Health { get; set; } = "GREEN";
        public int ManagerId { get; set; }
        public int TotalStoryPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User Manager { get; set; }
        public ICollection<Milestone> Milestones { get; set; }
        public ICollection<Allocation> Allocations { get; set; }
        public ICollection<Timesheet> Timesheets { get; set; }
    }

    public class Milestone
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public int StoryPoints { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Project Project { get; set; }
    }

    public class Allocation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int AllocatedBy { get; set; }
        public int UtilizationPct { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public Project Project { get; set; }
        public User Allocator { get; set; }

        // Backward compatibility helper
        [NotMapped]
        public int EmployeeId
        {
            get => UserId;
            set => UserId = value;
        }

        [NotMapped]
        public Employee Employee => new Employee
        {
            Id = UserId,
            UserId = UserId,
            FullName = User?.FullName ?? string.Empty,
            Email = User?.Email ?? string.Empty,
            Department = User?.Department ?? string.Empty,
            Designation = User?.Designation ?? string.Empty,
            IsActive = User?.IsActive ?? false,
            CreatedAt = User?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = User?.UpdatedAt ?? DateTime.UtcNow
        };
    }

    public class Timesheet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime WeekStart { get; set; }
        public float HoursLogged { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public User User { get; set; }
        public Project Project { get; set; }
        public User Approver { get; set; }
        public ICollection<TimesheetTag> TimesheetTags { get; set; }

        // Backward compatibility helper
        [NotMapped]
        public int EmployeeId
        {
            get => UserId;
            set => UserId = value;
        }

        [NotMapped]
        public Employee Employee => new Employee
        {
            Id = UserId,
            UserId = UserId,
            FullName = User?.FullName ?? string.Empty,
            Email = User?.Email ?? string.Empty,
            Department = User?.Department ?? string.Empty,
            Designation = User?.Designation ?? string.Empty,
            IsActive = User?.IsActive ?? false,
            CreatedAt = User?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = User?.UpdatedAt ?? DateTime.UtcNow
        };
    }

    public class ActivityTag
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<TimesheetTag> TimesheetTags { get; set; }
    }

    public class TimesheetTag
    {
        public int Id { get; set; }
        public int TimesheetId { get; set; }
        public int ActivityTagId { get; set; }

        public Timesheet Timesheet { get; set; }
        public ActivityTag ActivityTag { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }

    public class PrmDbContext : DbContext
    {
        public PrmDbContext(DbContextOptions<PrmDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Allocation> Allocations { get; set; }
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<ActivityTag> ActivityTags { get; set; }
        public DbSet<TimesheetTag> TimesheetTags { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Allocation relationships
            modelBuilder.Entity<Allocation>()
                .HasOne(a => a.Allocator)
                .WithMany(u => u.CreatedAllocations)
                .HasForeignKey(a => a.AllocatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Allocation>()
                .HasOne(a => a.User)
                .WithMany(u => u.Allocations)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Allocation>()
                .HasOne(a => a.Project)
                .WithMany(p => p.Allocations)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Timesheet relationships
            modelBuilder.Entity<Timesheet>()
                .HasOne(t => t.Approver)
                .WithMany(u => u.ApprovedTimesheets)
                .HasForeignKey(t => t.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Timesheet>()
                .HasOne(t => t.User)
                .WithMany(u => u.Timesheets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Timesheet>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Timesheets)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project.Manager -> User
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Manager)
                .WithMany(u => u.ManagedProjects)
                .HasForeignKey(p => p.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User self-referencing Manager relation
            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany(u => u.DirectReports)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRole configuration
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserSkill configuration
            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserStatus configuration
            modelBuilder.Entity<UserStatus>()
                .HasOne(us => us.User)
                .WithOne(u => u.Status)
                .HasForeignKey<UserStatus>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
