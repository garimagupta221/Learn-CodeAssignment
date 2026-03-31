using FinanceTracker.Adapters;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Services;
using FinanceTracker.Repositories;
using Microsoft.Graph;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddSingleton<IBudgetRepository, BudgetRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddSingleton<ITransactionService, TransactionService>();
builder.Services.AddSingleton<IBudgetService, BudgetService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IReportService, ReportService>();

builder.Services.AddSingleton<INotificationService, ConsoleNotificationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();