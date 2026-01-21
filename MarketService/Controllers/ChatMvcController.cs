using Microsoft.AspNetCore.Mvc;

namespace MarketService.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            if (!Request.Cookies.ContainsKey("auth_token"))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public IActionResult Conversation(int id)
        {
            if (!Request.Cookies.ContainsKey("auth_token"))
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.ConversationId = id;
            return View();
        }
    }
}
