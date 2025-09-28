// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using XIROX.Models;
using XIROX.Services;

namespace XIROX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // ===== Home =====
        [HttpGet]
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                TelegramUrl = "https://t.me/xiroxff",
                WhatsAppUrl = "https://wa.me/+989021581095",
                TikTokUrl = "https://www.tiktok.com/@xirox__ff",
                InstagramUrl = "https://www.instagram.com/xirox__ff",
                YouTubeUrl = "https://www.youtube.com/@xirox__ff",
                Email = "xiroxff80@gmail.com"
            };
            return View(model);
        }

        // ===== About / Privacy =====
        [HttpGet] public IActionResult About() => View();
        [HttpGet] public IActionResult Privacy() => View();

        // ===== Contact (GET) =====
        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        // ===== Contact (POST) - ارسال ایمیل =====
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            ContactViewModel model,
            [FromServices] IEmailSender mailer,
            [FromServices] IWebHostEnvironment env)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var subject = $"XIROX Contact — {model.Name}";
                await mailer.SendAsync(model.Name, model.Email, subject, model.Message);
                TempData["ContactSuccess"] = "Your message was sent. Thanks!";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Contact send failed.");
                TempData["ContactError"] = env.IsDevelopment()
                    ? $"Send failed: {ex.Message}"
                    : "Sorry, sending failed. Please try again later.";
            }

            return RedirectToAction(nameof(Contact));
        }

        // ===== Error =====
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(model);
        }

        // ===== Status Codes (404/500/...) =====
        [HttpGet]
        [Route("Home/StatusCode")]
        public IActionResult StatusPage([FromQuery] int code)
        {
            ViewBag.StatusCode = code;
            return View("StatusCode");
        }
    }
}
