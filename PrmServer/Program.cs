using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrmServer.Data;
using PrmServer.Entities;
using PrmServer.Extensions;
using PrmServer.Providers;
using PrmServer.Repositories;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services;
using PrmServer.Services.BackgroundTasks;
using PrmServer.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddSwaggerDocumentation();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Database
builder.Services.AddDbContext<PrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();
builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<ITimesheetTagRepository, TimesheetTagRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IActivityTagRepository, ActivityTagRepository>();
builder.Services.AddScoped<ITimesheetReminderRepository, TimesheetReminderRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<ISystemConfigService, SystemConfigService>();

// AI Providers (Factory / Strategy Pattern)
// Add a new IAiProvider implementation + register it here to support a new LLM — zero other changes needed (OCP).
// Active provider is resolved at runtime from system config key "ActiveAiProvider" (Admin-configurable).
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAiProvider, GemmaProvider>();   // default — self-hosted Ollama Gemma endpoint
builder.Services.AddScoped<IAiProvider, GeminiProvider>();
builder.Services.AddScoped<IAiProvider, GrokProvider>();
builder.Services.AddScoped<IAiService, AiService>();

// Background Services
// Each IScheduledTask is a Strategy — order determines execution sequence per cycle.
builder.Services.AddScoped<IScheduledTask, RecomputeUtilizationTask>();
builder.Services.AddScoped<IScheduledTask, EvaluateProjectHealthTask>();
builder.Services.AddScoped<IScheduledTask, MarkMissedTimesheetsTask>();
builder.Services.AddScoped<IScheduledTask, TimesheetReminderTask>();
builder.Services.AddHostedService<SystemSchedulerService>();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRM Tool API v1");
        c.DocumentTitle = "PRM Tool API";
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
