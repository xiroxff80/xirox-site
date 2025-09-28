namespace XIROX.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string subject, string body, string? fromName, string? fromEmail, CancellationToken ct = default);
    }
}