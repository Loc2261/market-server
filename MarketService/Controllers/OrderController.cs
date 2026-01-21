using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketService.Services;

namespace MarketService.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /Order/MyOrders
        public IActionResult MyOrders()
        {
            return View();
        }

        // GET: /Order/Details/{id}
        public IActionResult Details(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // GET: /Order/Track
        public IActionResult Track()
        {
            return View();
        }
    }
}
