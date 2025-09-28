using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XIROX.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration cfg, ILogger<SmtpEmailSender> logger)
        {
            _cfg = cfg;
            _logger = logger;
        }

        public async Task SendAsync(string subject, string body, string? fromName, string? fromEmail, CancellationToken ct = default)
        {
            var host     = _cfg["Smtp:Host"] ?? "smtp.gmail.com";
            var port     = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
            var startTls = bool.TryParse(_cfg["Smtp:UseStartTls"], out var s) ? s : true;
            var username = _cfg["Smtp:Username"];
            var password = _cfg["Smtp:Password"];
            var fromAddr = _cfg["Smtp:FromEmail"] ?? username ?? "";
            var toAddr   = _cfg["Smtp:ToEmail"]   ?? username ?? "";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl  = startTls,
                Credentials = string.IsNullOrWhiteSpace(username) ? null : new NetworkCredential(username, password),
                Timeout    = 20000
            };

            using var mail = new MailMessage();
            mail.From = new MailAddress(fromAddr, string.IsNullOrWhiteSpace(fromName) ? fromAddr : fromName);
            mail.To.Add(new MailAddress(toAddr));
            if (!string.IsNullOrWhiteSpace(fromEmail))
                mail.ReplyToList.Add(new MailAddress(fromEmail, string.IsNullOrWhiteSpace(fromName) ? fromEmail : fromName));

            mail.Subject = subject;
            mail.Body    = body;

            try
            {
                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP send failed");
                throw;
            }
        }
    }
}