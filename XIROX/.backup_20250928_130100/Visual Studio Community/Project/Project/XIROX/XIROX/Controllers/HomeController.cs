using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XIROX.Models;
using XIROX.Services;

namespace XIROX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _email;
        private readonly IConfiguration _cfg;

        public HomeController(ILogger<HomeController> logger, IEmailSender email, IConfiguration cfg)
        {
            _logger = logger;
            _email  = email;
            _cfg    = cfg;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new HomeViewModel
            {
                TelegramUrl  = _cfg["Links:Telegram"]  ?? "#",
                WhatsAppUrl  = _cfg["Links:WhatsApp"]  ?? "#",
                TikTokUrl    = _cfg["Links:TikTok"]    ?? "#",
                InstagramUrl = _cfg["Links:Instagram"] ?? "#",
                YouTubeUrl   = _cfg["Links:YouTube"]   ?? "#",
                DiscordUrl   = _cfg["Links:Discord"]   ?? "https://discord.gg/xiroxff"
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewBag.Success = TempData["Success"] as string;
            ViewBag.Error   = TempData["Error"] as string;
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var subject = $"Contact form - {model.Name}";
            var body =
$@"Name: {model.Name}
Email: {model.Email}

{model.Message}";
            try
            {
                await _email.SendAsync(subject, body, model.Name, model.Email,
                    HttpContext?.RequestAborted ?? default);
                TempData["Success"] = "پیام شما با موفقیت ارسال شد.";
                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send contact failed");
                TempData["Error"] = "ارسال ایمیل انجام نشد. لطفاً بعداً دوباره تلاش کنید.";
                return RedirectToAction(nameof(Contact));
            }
        }

        public IActionResult About()   => View();
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}