using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services.Shipping;
using MarketService.DTOs;

namespace MarketService.Controllers
{
    [Authorize]
    public class ShippingController : Controller
    {
        private readonly IShippingService _shippingService;
        private readonly IShippingProviderFactory _factory;

        public ShippingController(IShippingService shippingService, IShippingProviderFactory factory)
        {
            _shippingService = shippingService;
            _factory = factory;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _shippingService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        public async Task<IActionResult> Addresses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var addresses = await _shippingService.GetUserAddressesAsync(userId);
            return View(addresses);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddAddress(CreateAddressDTO dto)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _shippingService.AddAddressAsync(userId, dto);
            }
            return RedirectToAction("Addresses");
        }
    }
}
