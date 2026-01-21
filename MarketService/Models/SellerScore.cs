using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public class SellerScore
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        // Chỉ số tổng hợp (0.00 - 5.00)
        [Column(TypeName = "decimal(3,2)")]
        public decimal OverallScore { get; set; } = 0;

        // Số liệu giao dịch
        public int TotalSales { get; set; } = 0;
        public int CompletedOrders { get; set; } = 0;
        public int CancelledOrders { get; set; } = 0;

        // Hiệu suất
        [Column(TypeName = "decimal(10,2)")]
        public decimal? AverageResponseTime { get; set; } // Thời gian phản hồi TB (phút)

        [Column(TypeName = "decimal(5,2)")]
        public decimal CompletionRate { get; set; } = 0; // % hoàn thành

        // Đánh giá
        public int TotalReviews { get; set; } = 0;

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0;

        // Badges (JSON format)
        [StringLength(500)]
        public string? Badges { get; set; }

        public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
