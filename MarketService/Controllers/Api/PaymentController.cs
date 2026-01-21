using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketService.Services;
using MarketService.Models;
using System.Security.Claims;

namespace MarketService.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        [HttpGet("vnpay-url")]
        public async Task<IActionResult> GetVNPayUrl(int orderId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                // Verify order belongs to user
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null || order.BuyerId != userId)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });
                }

                // Create return URL (MVC action)
                var returnUrl = Url.Action("VNPayReturn", "Payment", null, Request.Scheme);
                
                var url = await _paymentService.CreateVNPayPaymentUrlAsync(orderId, returnUrl);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
