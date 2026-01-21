using System;
using System.Collections.Generic;

namespace MarketService.DTOs
{
    public class SellerDashboardStatsDTO
    {
        public decimal TodayRevenue { get; set; }
        public decimal YesterdayRevenue { get; set; } // To calculate growth
        public int PendingOrders { get; set; }
        public int TotalOrdersMonth { get; set; }
        public double AverageRating { get; set; }
        
        // Chart Data (Last 7 days)
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartData { get; set; } = new();
    }

    public class SellerAnalyticsDTO
    {
        // Monthly Revenue
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();

        // Order Status Distribution
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int ShippingCount { get; set; }
        public int ProcessingCount { get; set; }

        // Top Products
        public List<TopProductDTO> TopProducts { get; set; } = new();
    }

    public class TopProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SoldCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
