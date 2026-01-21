using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketService.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewBag.PostId = id;
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
            ViewBag.PostId = id;
            return View();
        }

        [Authorize]
        public IActionResult MyPosts()
        {
            return View();
        }
    }
}
