using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Application.Interfaces
{
    public interface IBudgetRepository
    {
        void AddOrUpdate(Budget budget);
        Budget GetBudget(Guid userId, Category category, int month, int year);
    }
}
