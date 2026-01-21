using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services.Shipping;
using MarketService.DTOs;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly IShippingProviderFactory _providerFactory;

        public ShippingController(
            IShippingService shippingService,
            IShippingProviderFactory providerFactory)
        {
            _shippingService = shippingService;
            _providerFactory = providerFactory;
        }

        // ========== Address Management ==========

        // POST api/shipping/addresses
        [Authorize]
        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] CreateAddressDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var address = await _shippingService.AddAddressAsync(userId, dto);
            return Ok(address);
        }

        // GET api/shipping/addresses
        [Authorize]
        [HttpGet("addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var addresses = await _shippingService.GetUserAddressesAsync(userId);
            return Ok(addresses);
        }

        // PUT api/shipping/addresses/{id}/set-default
        [Authorize]
        [HttpPut("addresses/{id}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _shippingService.SetDefaultAddressAsync(userId, id);
            
            if (!success) return NotFound();
            return Ok(new { message = "Đã đặt làm địa chỉ mặc định" });
        }

        // DELETE api/shipping/addresses/{id}
        [Authorize]
        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _shippingService.DeleteAddressAsync(userId, id);
            
            if (!success) return NotFound();
            return Ok(new { message = "Đã xóa địa chỉ" });
        }

        // ========== Fee Calculation ==========

        // GET api/shipping/calculate-all?productId=1&addressId=2
        [Authorize]
        [HttpGet("calculate-all")]
        public async Task<IActionResult> CalculateAllProviders([FromQuery] int productId, [FromQuery] int addressId)
        {
            try
            {
                var fees = await _shippingService.CalculateAllProvidersAsync(productId, addressId);
                return Ok(fees);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/shipping/calculate?productId=1&addressId=2&provider=GHN
        [Authorize]
        [HttpGet("calculate")]
        public async Task<IActionResult> CalculateFee(
            [FromQuery] int productId, 
            [FromQuery] int addressId, 
            [FromQuery] string provider)
        {
            try
            {
                var fee = await _shippingService.CalculateFeeAsync(productId, addressId, provider);
                return Ok(new { fee, provider });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ========== Order Management ==========

        // POST api/shipping/orders
        [Authorize]
        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateShippingOrderDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                dto.BuyerId = userId; // Ensure buyer is current user

                var order = await _shippingService.CreateShippingOrderAsync(dto);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/shipping/orders
        [Authorize]
        [HttpGet("orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _shippingService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        // DELETE api/shipping/orders/{id}
        [Authorize]
        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
             var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
             var result = await _shippingService.DeleteOrderAsync(id, userId);
             
             if (!result) return NotFound(new { message = "Không tìm thấy đơn hàng hoặc bạn không có quyền xóa" });
             
             return Ok(new { message = "Đã xóa đơn hàng" });
        }

        // GET api/shipping/track/{trackingNumber}
        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> TrackOrder(string trackingNumber)
        {
            try
            {
                var trackingInfo = await _shippingService.TrackOrderAsync(trackingNumber);
                return Ok(trackingInfo);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ========== Providers Info ==========

        // GET api/shipping/providers
        [HttpGet("providers")]
        public IActionResult GetAvailableProviders()
        {
            var providers = _providerFactory.GetAvailableProviders();
            return Ok(new { providers });
        }
    }
}
