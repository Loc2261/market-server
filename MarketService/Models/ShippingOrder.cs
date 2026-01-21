using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public enum ShippingStatus
    {
        Pending,
        PickedUp,
        InTransit,
        Delivered,
        Failed,
        Cancelled
    }

    public class ShippingOrder
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        public int BuyerId { get; set; }

        // Thông tin vận chuyển
        [Required]
        [StringLength(20)]
        public string Provider { get; set; } = string.Empty; // 'GHN', 'GHTK'

        [StringLength(100)]
        public string? TrackingNumber { get; set; } // Mã vận đơn

        [Required]
        [StringLength(1000)]
        public string PickupAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Required]
        public ShippingStatus Status { get; set; } = ShippingStatus.Pending;

        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false; // Legacy/General deletion
        public bool IsDeletedByBuyer { get; set; } = false;
        public bool IsDeletedBySeller { get; set; } = false;

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; } = null!;

        [ForeignKey("BuyerId")]
        public virtual User Buyer { get; set; } = null!;
    }
}
