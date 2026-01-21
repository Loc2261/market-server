using Microsoft.AspNetCore.Mvc;

namespace MarketService.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            if (Request.Cookies.ContainsKey("auth_token"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Register()
        {
            if (Request.Cookies.ContainsKey("auth_token"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult ForgotPassword()
        {
            if (Request.Cookies.ContainsKey("auth_token"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }
            ViewBag.Token = token;
            return View();
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token");
            return RedirectToAction("Index", "Home");
        }
    }
}
