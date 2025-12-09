using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpSettings> smtpSettings, ILogger<SmtpEmailSender> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_smtpSettings.Host) || string.IsNullOrEmpty(_smtpSettings.User))
            {
                _logger.LogWarning("SMTP settings are not configured. Email to {Email} was not sent.", email);
                return;
            }

            try
            {
                using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to {Email} successfully.", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}.", email);
                throw;
            }
        }
    }
}
