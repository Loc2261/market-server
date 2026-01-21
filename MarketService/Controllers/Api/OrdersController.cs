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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public OrdersController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                var order = await _orderService.CreateOrderFromCartAsync(
                    userId,
                    request.ShippingAddressId,
                    request.PaymentMethod,
                    request.ShippingProvider,
                    request.ShippingFee,
                    request.Note
                );

                // Process payment based on method
                if (request.PaymentMethod == PaymentMethod.COD)
                {
                    await _paymentService.ProcessCODPaymentAsync(order.Id);
                }

                return Ok(new { orderId = order.Id, message = "Đặt hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.GetOrderDetailsAsync(id, userId);

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng" });
            }

            return Ok(order);
        }

        // GET api/orders/buyer
        [HttpGet("buyer")]
        public async Task<IActionResult> GetBuyerOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _orderService.GetBuyerOrdersAsync(userId, page, pageSize);
            return Ok(result);
        }

        // GET api/orders/seller
        [HttpGet("seller")]
        public async Task<IActionResult> GetSellerOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] List<OrderStatus>? statuses = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _orderService.GetSellerOrdersAsync(userId, page, pageSize, statuses);
            return Ok(result);
        }

        // PUT api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _orderService.UpdateOrderStatusAsync(id, userId, request.Status);

            if (!success)
            {
                return BadRequest(new { message = "Không thể cập nhật trạng thái đơn hàng" });
            }

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }

        // DELETE api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _orderService.CancelOrderAsync(id, userId, request.Reason ?? "Không có lý do");

            if (!success)
            {
                return BadRequest(new { message = "Không thể hủy đơn hàng" });
            }

            return Ok(new { message = "Hủy đơn hàng thành công" });
        }

        // POST api/orders/{id}/confirm-delivery
        [HttpPost("{id}/confirm-delivery")]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _orderService.ConfirmDeliveryAsync(id, userId);

            if (!success)
            {
                return BadRequest(new { message = "Không thể xác nhận đã nhận hàng" });
            }

            return Ok(new { message = "Xác nhận thành công" });
        }
        // GET api/orders/seller/stats/dashboard
        [HttpGet("seller/stats/dashboard")]
        public async Task<IActionResult> GetSellerDashboardStats()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var stats = await _orderService.GetSellerDashboardStatsAsync(userId);
            return Ok(stats);
        }

        // GET api/orders/seller/stats/analytics
        [HttpGet("seller/stats/analytics")]
        public async Task<IActionResult> GetSellerAnalytics()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var stats = await _orderService.GetSellerAnalyticsAsync(userId);
            return Ok(stats);
        }

        // POST api/orders/{id}/simulate-delivery
        [HttpPost("{id}/simulate-delivery")]
        public async Task<IActionResult> SimulateDelivery(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _orderService.SimulateDeliveryAsync(id, userId);

            if (!success)
            {
                return BadRequest(new { message = "Không thể mô phỏng giao hàng (Đơn hàng phải ở trạng thái Đang giao)" });
            }

            return Ok(new { message = "Đã cập nhật trạng thái đã giao hàng" });
        }
    }

    public class CreateOrderRequest
    {
        public int ShippingAddressId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ShippingProvider { get; set; }
        public decimal ShippingFee { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateStatusRequest
    {
        public OrderStatus Status { get; set; }
    }

    public class CancelOrderRequest
    {
        public string? Reason { get; set; }
    }
}
