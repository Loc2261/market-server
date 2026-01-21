using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        // Địa chỉ 3 cấp
        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty; // Tỉnh/TP

        [Required]
        [StringLength(100)]
        public string District { get; set; } = string.Empty; // Quận/Huyện

        [StringLength(100)]
        public string? Ward { get; set; } // Phường/Xã

        [Required]
        [StringLength(500)]
        public string AddressLine { get; set; } = string.Empty; // Số nhà, đường

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
