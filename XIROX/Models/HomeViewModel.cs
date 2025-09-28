using System.ComponentModel.DataAnnotations;

namespace XIROX.Models
{
    public class HomeViewModel
    {
        // لینک تلگرام
        [Required(ErrorMessage = "Telegram link is required."), Url]
        public string TelegramUrl { get; set; } = string.Empty;

        // لینک واتساپ
        [Required(ErrorMessage = "WhatsApp link is required."), Url]
        public string WhatsAppUrl { get; set; } = string.Empty;

        // لینک تیک‌تاک
        [Required(ErrorMessage = "TikTok link is required."), Url]
        public string TikTokUrl { get; set; } = string.Empty;

        // لینک اینستاگرام
        [Required(ErrorMessage = "Instagram link is required."), Url]
        public string InstagramUrl { get; set; } = string.Empty;

        // لینک یوتیوب
        [Required(ErrorMessage = "YouTube link is required."), Url]
        public string YouTubeUrl { get; set; } = string.Empty;

        // ایمیل
        [Required(ErrorMessage = "Email is required."), EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
