using MarketService.Models;
using MarketService.DTOs;

namespace MarketService.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderFromCartAsync(int buyerId, int shippingAddressId, PaymentMethod paymentMethod, string? shippingProvider = null, decimal shippingFee = 0, string? note = null);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<PagedResult<Order>> GetBuyerOrdersAsync(int buyerId, int page = 1, int pageSize = 10);
        Task<PagedResult<SellerOrderViewDTO>> GetSellerOrdersAsync(int sellerId, int page = 1, int pageSize = 10, List<OrderStatus>? statuses = null);
        Task<bool> UpdateOrderStatusAsync(int orderId, int userId, OrderStatus newStatus);
        Task<bool> CancelOrderAsync(int orderId, int userId, string reason);
        Task<bool> ConfirmDeliveryAsync(int orderId, int buyerId);
        Task<Order?> GetOrderDetailsAsync(int orderId, int userId);
        Task<MarketService.DTOs.SellerDashboardStatsDTO> GetSellerDashboardStatsAsync(int sellerId);
        Task<MarketService.DTOs.SellerAnalyticsDTO> GetSellerAnalyticsAsync(int sellerId);
        Task<bool> SimulateDeliveryAsync(int orderId, int userId);
    }
}
