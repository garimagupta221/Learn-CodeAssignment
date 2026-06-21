namespace PrmServer.Services.Interfaces
{
    /// <summary>
    /// Abstraction for sending outbound emails.
    /// Implementations must never throw — failures are logged and swallowed so that
    /// an email send error cannot crash a background scheduler task.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="toName">Recipient display name (used in the To: header).</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="htmlBody">Full HTML body of the email.</param>
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    }
}
