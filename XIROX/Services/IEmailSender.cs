// Services/IEmailSender.cs
using System.Threading;
using System.Threading.Tasks;

namespace XIROX.Services
{
    /// <summary>
    /// لایه‌ی انتزاع ارسال ایمیل از فرم Contact
    /// </summary>
    public interface IEmailSender
    {
        /// <param name="fromName">نام ارسال‌کننده (کاربر فرم)</param>
        /// <param name="fromEmail">ایمیل ارسال‌کننده (کاربر فرم)</param>
        /// <param name="subject">موضوع دلخواه (می‌تواند null باشد)</param>
        /// <param name="message">متن پیام</param>
        Task SendAsync(string fromName, string fromEmail, string? subject, string? message, CancellationToken ct = default);
    }
}
