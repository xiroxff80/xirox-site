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

        public async Task SendContactAsync(string name, string fromEmail, string message)
        {
            var host      = _cfg["Smtp:Host"] ?? "smtp.gmail.com";
            var portStr   = _cfg["Smtp:Port"];
            var useStart  = _cfg["Smtp:UseStartTls"];
            var username  = _cfg["Smtp:Username"];
            var password  = _cfg["Smtp:Password"];
            var fromAddr  = _cfg["Smtp:FromEmail"] ?? username;
            var toAddr    = _cfg["Smtp:ToEmail"]   ?? username;

            int port = 587;
            bool.TryParse(useStart, out var enableSsl);
            int.TryParse(portStr, out port);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl, // روی 587 یعنی STARTTLS
                Credentials = new NetworkCredential(username, password),
                Timeout = 20000
            };

            using var mail = new MailMessage();
            mail.From = new MailAddress(fromAddr!);
            mail.To.Add(new MailAddress(toAddr!));
            if (!string.IsNullOrWhiteSpace(fromEmail))
                mail.ReplyToList.Add(new MailAddress(fromEmail));

            mail.Subject = $"Contact form - {name}";
            mail.Body =
$@"Name: {name}
Email: {fromEmail}

{message}";

            try
            {
                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP send failed");
                throw; // کنترلر به‌نرمی هندل می‌کند (دیگه 502 نمی‌بینی)
            }
        }
    }
}