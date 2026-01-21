using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketService.Services;

namespace MarketService.Controllers
{
    [Authorize]
    public class SellerController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public SellerController(IOrderService orderService, IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        // GET: /Seller/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: /Seller/Orders
        public IActionResult Orders()
        {
            return View();
        }

        // GET: /Seller/Products
        public IActionResult Products()
        {
            return View();
        }

        // GET: /Seller/Analytics
        public IActionResult Analytics()
        {
            return View();
        }
    }
}
