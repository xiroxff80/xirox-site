using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XIROX.Models;
using XIROX.Services;

namespace XIROX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _email;

        public HomeController(ILogger<HomeController> logger, IEmailSender email)
        {
            _logger = logger;
            _email = email;
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewBag.Success = TempData["Success"] as string;
            ViewBag.Error = TempData["Error"] as string;
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _email.SendContactAsync(model.Name!, model.Email!, model.Message!);
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

        public IActionResult Index() => View();
        public IActionResult About() => View();
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}