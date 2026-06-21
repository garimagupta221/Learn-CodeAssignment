using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services
{
    /// <summary>
    /// Sends emails via an SMTP relay.
    /// Configuration is read from the "Email" section of appsettings.json:
    ///   Host, Port, Username, Password, From, EnableSsl.
    /// All exceptions are caught and logged so that a send failure can never
    /// crash a background scheduler task.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("[Email] Attempted to send '{Subject}' but recipient email is empty. Skipping.", subject);
                return;
            }

            try
            {
                var section = _config.GetSection("Email");
                var host       = section["Host"]     ?? throw new InvalidOperationException("Email:Host not configured.");
                var portStr    = section["Port"]     ?? "587";
                var username   = section["Username"] ?? string.Empty;
                var password   = section["Password"] ?? string.Empty;
                var from       = section["From"]     ?? username;
                var enableSsl  = bool.TryParse(section["EnableSsl"], out var ssl) && ssl;

                if (!int.TryParse(portStr, out int port))
                    port = 587;

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl   = enableSsl,
                    Credentials = string.IsNullOrWhiteSpace(username)
                        ? null
                        : new NetworkCredential(username, password),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                using var message = new MailMessage
                {
                    From       = new MailAddress(from),
                    Subject    = subject,
                    Body       = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(toEmail, toName));

                await client.SendMailAsync(message);

                _logger.LogInformation("[Email] Sent '{Subject}' to {ToEmail}.", subject, toEmail);
            }
            catch (Exception ex)
            {
                // Failures are logged but never re-thrown — email is informational and
                // must not block core business logic or background task pipelines.
                _logger.LogError(ex, "[Email] Failed to send '{Subject}' to {ToEmail}.", subject, toEmail);
            }
        }
    }
}
