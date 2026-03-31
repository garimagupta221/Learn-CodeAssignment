using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTracker.Domain.Enums;


namespace FinanceTracker.Application.DTOs
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
