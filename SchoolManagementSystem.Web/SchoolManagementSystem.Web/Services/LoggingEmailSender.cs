using Microsoft.AspNetCore.Identity.UI.Services;

namespace SchoolManagementSystem.Web.Services
{
    public class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("--------------------------------------------------");
            _logger.LogInformation("Sending email to {Email}", email);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {HtmlMessage}", htmlMessage);
            _logger.LogInformation("--------------------------------------------------");
            return Task.CompletedTask;
        }
    }
}
