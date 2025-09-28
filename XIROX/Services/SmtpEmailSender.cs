// Services/SmtpEmailSender.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XIROX.Services
{
    /// <summary>
    /// SMTP options bound from appsettings (section: "Smtp")
    /// </summary>
    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;

        public string Username { get; set; } = string.Empty;   // Gmail address
        public string Password { get; set; } = string.Empty;   // App Password (ENV on server)
        public string FromName { get; set; } = "XIROX Website";
        public string FromEmail { get; set; } = string.Empty;    // same as Username usually
        public string ToEmail { get; set; } = string.Empty;    // where messages arrive
    }

    /// <summary>
    /// Sends plain text messages via SMTP (Gmail App Password supported).
    /// </summary>
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<SmtpEmailSender> _log;

        public SmtpEmailSender(IOptions<SmtpOptions> opt, ILogger<SmtpEmailSender> log)
        {
            _opt = opt.Value;
            _log = log;
        }

        public async Task SendAsync(
            string fromName,
            string fromEmail,
            string? subject,
            string? message,
            CancellationToken ct = default)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(_opt.Host)) throw new InvalidOperationException("SMTP Host is empty.");
            if (string.IsNullOrWhiteSpace(_opt.Username)) throw new InvalidOperationException("SMTP Username is empty.");
            if (string.IsNullOrWhiteSpace(_opt.Password)) throw new InvalidOperationException("SMTP Password is empty.");
            if (string.IsNullOrWhiteSpace(_opt.FromEmail)) throw new InvalidOperationException("SMTP FromEmail is empty.");
            if (string.IsNullOrWhiteSpace(_opt.ToEmail)) throw new InvalidOperationException("SMTP ToEmail is empty.");

            // Build message
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName ?? "XIROX Website", _opt.FromEmail));
            msg.To.Add(new MailboxAddress("XIROX", _opt.ToEmail));

            // Let inbox "Reply" go directly to the visitor
            if (!string.IsNullOrWhiteSpace(fromEmail))
                msg.ReplyTo.Add(new MailboxAddress(string.IsNullOrWhiteSpace(fromName) ? fromEmail : fromName, fromEmail));

            msg.Subject = !string.IsNullOrWhiteSpace(subject) ? subject.Trim() : $"Contact Form - {fromName}";

            msg.Body = new TextPart("plain")
            {
                Text =
$@"From: {fromName} <{fromEmail}>
Time (UTC): {DateTime.UtcNow:O}

Message:
{message}"
            };

            // Send
            using var client = new SmtpClient
            {
                Timeout = 15000,
                CheckCertificateRevocation = true
            };

            // With App Password we don't use XOAUTH2
            client.AuthenticationMechanisms.Remove("XOAUTH2");

            var mode = SecureSocketOptions.Auto;
            if (_opt.Port == 465) mode = SecureSocketOptions.SslOnConnect;
            else if (_opt.UseStartTls) mode = SecureSocketOptions.StartTls;

            try
            {
                _log.LogInformation("SMTP connect {Host}:{Port} (Mode={Mode})", _opt.Host, _opt.Port, mode);
                await client.ConnectAsync(_opt.Host, _opt.Port, mode, ct);

                _log.LogInformation("SMTP authenticate as {User}", _opt.Username);
                await client.AuthenticateAsync(_opt.Username, _opt.Password, ct);

                _log.LogInformation("SMTP send → {To}", _opt.ToEmail);
                await client.SendAsync(msg, ct);

                await client.DisconnectAsync(true, ct);
                _log.LogInformation("SMTP sent successfully.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SMTP send failed: {Message}", ex.Message);
                throw; // bubble up; controller will map to friendly message in Production
            }
        }
    }
}
