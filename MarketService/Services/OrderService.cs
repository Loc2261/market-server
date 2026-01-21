using MarketService.Data;
using MarketService.Models;
using Microsoft.EntityFrameworkCore;
using MarketService.DTOs;
using MarketService.Helpers;

namespace MarketService.Services
{
    public class OrderService : IOrderService
    {
        private readonly MarketDbContext _context;
        private readonly ICartService _cartService;
        private readonly INotificationService _notificationService;
        private readonly Services.Shipping.IShippingService _shippingService;

        public OrderService(
            MarketDbContext context,
            ICartService cartService,
            INotificationService notificationService,
            Services.Shipping.IShippingService shippingService)
        {
            _context = context;
            _cartService = cartService;
            _notificationService = notificationService;
            _shippingService = shippingService;
        }

        public async Task<Order> CreateOrderFromCartAsync(
            int buyerId,
            int shippingAddressId,
            PaymentMethod paymentMethod,
            string? shippingProvider = null,
            decimal shippingFee = 0,
            string? note = null)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == buyerId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                throw new InvalidOperationException("Giỏ hàng trống");
            }

            // Group cart items by seller
            var ordersBySeller = new List<Order>();
            var itemsBySeller = cartItems.GroupBy(ci => ci.Product.SellerId);

            foreach (var sellerGroup in itemsBySeller)
            {
                var sellerId = sellerGroup.Key;
                var items = sellerGroup.ToList();
                var firstProduct = items.First();

                var totalAmount = items.Sum(i => i.Product.Price * i.Quantity);
                
                // Use provided or calculate shipping fee
                var providerName = shippingProvider ?? "GHN"; 
                var fee = shippingFee > 0 ? shippingFee : await _shippingService.CalculateFeeAsync(
                    firstProduct.ProductId, 
                    shippingAddressId, 
                    providerName
                );

                var order = new Order
                {
                    BuyerId = buyerId,
                    SellerId = sellerId,
                    TotalAmount = totalAmount,
                    ShippingFee = fee,
                    FinalAmount = totalAmount + fee,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = PaymentStatus.Unpaid,
                    ShippingAddressId = shippingAddressId,
                    ShippingProvider = providerName,
                    Note = note,
                    Status = OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    // Simulate Tracking Number
                    TrackingNumber = $"{providerName}{DateTime.Now:yyMMdd}{new Random().Next(1000,9999)}"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create Shipping Order Record
                try {
                     await _shippingService.CreateShippingOrderAsync(new DTOs.CreateShippingOrderDTO {
                         ProductId = firstProduct.ProductId,
                         PickupAddressId = 0, // Fallback to seller default in ShippingService
                         DeliveryAddressId = shippingAddressId,
                         BuyerId = buyerId,
                         Provider = providerName,
                         ShippingFee = fee
                     });
                } catch {}

                // Create order items
                foreach (var cartItem in items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Title,
                        ProductImageUrl = cartItem.Product.ImageUrl,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.Product.Price,
                        Subtotal = cartItem.Product.Price * cartItem.Quantity
                    };

                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                ordersBySeller.Add(order);

                // Send notification to seller
                await _notificationService.CreateNotificationAsync(
                    sellerId,
                    "new_order",
                    $"Bạn có đơn hàng mới #{order.Id}",
                    $"/Order/SellerOrders/{order.Id}"
                );
            }

            // Clear cart using DB directly to avoid recursive service calls if any
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return ordersBySeller.First();
        }


        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<PagedResult<Order>> GetBuyerOrdersAsync(int buyerId, int page = 1, int pageSize = 10)
        {
            var query = _context.Orders
                .Include(o => o.Seller)
                .Include(o => o.OrderItems)
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.OrderDate);

            return await query.ToPagedResultAsync(page, pageSize, o => o);
        }

        public async Task<PagedResult<SellerOrderViewDTO>> GetSellerOrdersAsync(int sellerId, int page = 1, int pageSize = 10, List<OrderStatus>? statuses = null)
        {
            var query = _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                .Where(o => o.SellerId == sellerId);

            if (statuses != null && statuses.Any())
            {
                query = query.Where(o => statuses.Contains(o.Status));
            }

            query = query.OrderByDescending(o => o.OrderDate);

            return await query.ToPagedResultAsync(page, pageSize, o => new SellerOrderViewDTO
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status,
                FinalAmount = o.FinalAmount,
                PaymentMethod = o.PaymentMethod,
                BuyerName = o.Buyer?.FullName ?? o.Buyer?.Username ?? "Khách lẻ", 
                OrderItems = (o.OrderItems ?? new List<OrderItem>()).Select(i => new SellerOrderItemDTO
                {
                    ProductName = i.ProductName,
                    ProductImageUrl = i.ProductImageUrl ?? string.Empty,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            });
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, int userId, OrderStatus newStatus)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return false;

            // Only seller can update status (except cancellation and delivery confirmation)
            if (order.SellerId != userId && newStatus != OrderStatus.Cancelled)
            {
                return false;
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            switch (newStatus)
            {
                case OrderStatus.Shipping:
                    order.ShippedDate = DateTime.UtcNow;
                    await _notificationService.CreateNotificationAsync(
                        order.BuyerId,
                        "order_shipped",
                        $"Đơn hàng #{order.Id} đang được vận chuyển",
                        $"/Order/Details/{order.Id}"
                    );
                    break;

                case OrderStatus.Delivered:
                    order.DeliveredDate = DateTime.UtcNow;
                    await _notificationService.CreateNotificationAsync(
                        order.BuyerId,
                        "order_delivered",
                        $"Đơn hàng #{order.Id} đã được giao",
                        $"/Order/Details/{order.Id}"
                    );
                    break;

                case OrderStatus.Confirmed:
                    await _notificationService.CreateNotificationAsync(
                        order.BuyerId,
                        "order_confirmed",
                        $"Đơn hàng #{order.Id} đã được xác nhận",
                        $"/Order/Details/{order.Id}"
                    );
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId, int userId, string reason)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return false;

            // Buyer or seller can cancel
            if (order.BuyerId != userId && order.SellerId != userId)
            {
                return false;
            }

            // Can only cancel if order is Pending or Confirmed
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return false;
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelledDate = DateTime.UtcNow;
            order.CancellationReason = reason;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify the other party
            var notifyUserId = order.BuyerId == userId ? order.SellerId : order.BuyerId;
            await _notificationService.CreateNotificationAsync(
                notifyUserId,
                "order_cancelled",
                $"Đơn hàng #{order.Id} đã bị hủy",
                $"/Order/Details/{order.Id}"
            );

            return true;
        }

        public async Task<bool> ConfirmDeliveryAsync(int orderId, int buyerId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null || order.BuyerId != buyerId) return false;

            if (order.Status != OrderStatus.Delivered)
            {
                return false;
            }

            order.Status = OrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                order.SellerId,
                "order_completed",
                $"Đơn hàng #{order.Id} đã hoàn thành",
                $"/Seller/Orders/{order.Id}"
            );

            return true;
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId, int userId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return null;

            // User must be buyer or seller
            if (order.BuyerId != userId && order.SellerId != userId)
            {
                return null;
            }

            return order;
        }
        public async Task<MarketService.DTOs.SellerDashboardStatsDTO> GetSellerDashboardStatsAsync(int sellerId)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var yesterday = today.AddDays(-1);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var orders = await _context.Orders
                .Where(o => o.SellerId == sellerId)
                .ToListAsync();

            var stats = new MarketService.DTOs.SellerDashboardStatsDTO
            {
                TodayRevenue = orders.Where(o => o.OrderDate >= today && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded).Sum(o => o.FinalAmount),
                YesterdayRevenue = orders.Where(o => o.OrderDate >= yesterday && o.OrderDate < today && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded).Sum(o => o.FinalAmount),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing),
                TotalOrdersMonth = orders.Count(o => o.OrderDate >= monthStart),
                AverageRating = 5.0 // Placeholder, implement real review avg later
            };

            // Chart Data (Last 7 days)
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var nextDate = date.AddDays(1);
                var revenue = orders
                    .Where(o => o.OrderDate >= date && o.OrderDate < nextDate && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
                    .Sum(o => o.FinalAmount);
                
                stats.ChartLabels.Add(date.ToString("dd/MM"));
                stats.ChartData.Add(revenue);
            }

            return stats;
        }

        public async Task<MarketService.DTOs.SellerAnalyticsDTO> GetSellerAnalyticsAsync(int sellerId)
        {
            var analytics = new MarketService.DTOs.SellerAnalyticsDTO();
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.SellerId == sellerId)
                .ToListAsync();

            // Status Distribution
            analytics.CompletedCount = orders.Count(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered);
            analytics.CancelledCount = orders.Count(o => o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Refunded);
            analytics.ShippingCount = orders.Count(o => o.Status == OrderStatus.Shipping);
            analytics.ProcessingCount = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing);

            // Monthly Revenue (Last 6 months)
            var now = DateTime.UtcNow;
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                
                var revenue = orders
                    .Where(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
                    .Sum(o => o.FinalAmount);
                
                analytics.MonthlyLabels.Add(monthDate.ToString("MM/yyyy"));
                analytics.MonthlyRevenue.Add(revenue);
            }

            // Top Products
            var allItems = orders
                .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
                .SelectMany(o => o.OrderItems)
                .GroupBy(i => i.ProductId)
                .Select(g => new MarketService.DTOs.TopProductDTO
                {
                    ProductId = g.Key,
                    ProductName = g.First().ProductName,
                    SoldCount = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.SoldCount)
                .Take(5)
                .ToList();

            analytics.TopProducts = allItems;

            return analytics;
        }

        public async Task<bool> SimulateDeliveryAsync(int orderId, int userId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            // Only seller can simulate delivery
            if (order.SellerId != userId) return false;

            // Must be in Shipping state
            if (order.Status != OrderStatus.Shipping) return false;

            order.Status = OrderStatus.Delivered;
            order.DeliveredDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                order.BuyerId,
                "order_delivered",
                $"Đơn hàng #{order.Id} đã giao hàng thành công (Mô phỏng)",
                $"/Order/Details/{order.Id}"
            );

            return true;
        }
    }
}
