using FinanceTracker.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Adapters
{
    public class ConsoleNotificationService : INotificationService
    {
        public void SendNotification(string message)
        {
            Console.WriteLine($"[Notification]: {message}");
        }
    }
}
