using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public enum OrderStatus
    {
        Pending,        // Chờ xác nhận
        Confirmed,      // Đã xác nhận
        Processing,     // Đang chuẩn bị hàng
        Shipping,       // Đang vận chuyển
        Delivered,      // Đã giao hàng
        Completed,      // Hoàn thành
        Cancelled,      // Đã hủy
        Refunded        // Đã hoàn tiền
    }

    public enum PaymentMethod
    {
        COD,            // Cash on Delivery
        VNPay,          // VNPay
        MoMo,           // MoMo
        BankTransfer    // Chuyển khoản
    }

    public enum PaymentStatus
    {
        Unpaid,         // Chưa thanh toán
        Paid,           // Đã thanh toán
        Refunded,       // Đã hoàn tiền
        Failed          // Thanh toán thất bại
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        public int SellerId { get; set; }

        // Order Information
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Payment Information
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        // Shipping Information
        [Required]
        public int ShippingAddressId { get; set; }
        
        [StringLength(100)]
        public string? ShippingProvider { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("BuyerId")]
        public virtual User Buyer { get; set; } = null!;

        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; } = null!;

        [ForeignKey("ShippingAddressId")]
        public virtual ShippingAddress ShippingAddress { get; set; } = null!;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
