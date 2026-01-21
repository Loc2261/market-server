using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardStatsDTO> GetDashboardStatsAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly MarketDbContext _context;

        public AdminService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardStatsDTO> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
            
            // Sử dụng Order để thống kê doanh thu và đơn hàng thành công
            var successfulOrders = await _context.Orders.CountAsync(o => 
                o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered);
            
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.FinalAmount);

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new ActivityResponseDTO
                {
                    Type = "User",
                    Detail = $"{u.FullName ?? u.Username} vừa đăng ký",
                    Time = u.CreatedAt,
                    Status = "Mới"
                })
                .ToListAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.Buyer)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new ActivityResponseDTO
                {
                    Type = "Order",
                    Detail = $"Đơn hàng từ {(o.Buyer != null ? (o.Buyer.FullName ?? o.Buyer.Username) : "Khách")}",
                    Time = o.CreatedAt,
                    Status = o.Status.ToString()
                })
                .ToListAsync();

            var activities = recentUsers.Concat(recentOrders)
                .OrderByDescending(a => a.Time)
                .Take(10)
                .ToList();

            return new AdminDashboardStatsDTO
            {
                TotalUsers = totalUsers,
                ActiveProducts = activeProducts,
                SuccessfulOrders = successfulOrders,
                TotalRevenue = totalRevenue,
                RecentActivities = activities
            };
        }
    }
}
