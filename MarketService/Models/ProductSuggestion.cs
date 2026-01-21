using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public class ProductSuggestion
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? ProductId { get; set; } // NULL nếu chưa tạo product

        // Gợi ý từ AI
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SuggestedPrice { get; set; }

        [StringLength(100)]
        public string? SuggestedCategory { get; set; }

        [StringLength(200)]
        public string? SuggestedTitle { get; set; }

        [StringLength(2000)]
        public string? SuggestedDescription { get; set; }

        // Độ chính xác (0.00 - 1.00)
        [Column(TypeName = "decimal(3,2)")]
        public decimal? ConfidenceScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
