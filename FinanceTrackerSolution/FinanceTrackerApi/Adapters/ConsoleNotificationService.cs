using FinanceTrackerApi.Application.Interfaces;

namespace FinanceTrackerApi.Adapters
{
    public class ConsoleNotificationService : INotificationService
    {
        public void SendNotification(string message)
        {
            Console.WriteLine($"[Notification]: {message}");
        }
    }
}
