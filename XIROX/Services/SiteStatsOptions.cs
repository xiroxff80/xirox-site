// Services/SiteStatsOptions.cs
namespace XIROX.Services
{
    /// <summary>
    /// تنظیمات آمار سایت (مسیر فایل JSON و تاریخ شروع سایت برای Days Online)
    /// </summary>
    public sealed class SiteStatsOptions
    {
        /// <summary>
        /// مسیر فایل JSON برای ذخیره آمار (Relative به روت وب‌اپ)
        /// مثال: "App_Data/stats.json"
        /// </summary>
        public string DataFile { get; set; } = "App_Data/stats.json";

        /// <summary>
        /// تاریخ شروع سایت (UTC). برای محاسبه‌ی Days Online.
        /// </summary>
        public DateTime SiteBirthday { get; set; } = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
