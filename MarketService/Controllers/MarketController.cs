using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketService.Controllers
{
    public class MarketController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        public IActionResult Edit(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }

        [Authorize]
        public IActionResult MyProducts()
        {
            return View();
        }

        [Authorize]
        public IActionResult Cart()
        {
            return View();
        }

        [Authorize]
        public IActionResult Wishlist()
        {
            return View();
        }

        [Authorize]
        public IActionResult Checkout()
        {
            return View();
        }

    }
}
