using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Services
{
    public class BudgetService : IBudgetService
    {
        public BudgetService(IBudgetRepository repo)
        {
            _repo = repo;
        }

        public Budget SetBudget(BudgetRequest request)
        {
            var budget = new Budget
            {
                UserId = request.UserId,
                Category = request.Category,
                MonthlyLimit = request.MonthlyLimit,
                Month = request.Month,
                Year = request.Year
            };

            _repo.AddOrUpdate(budget);
            return budget;
        }

        private readonly IBudgetRepository _repo;

    }
}
