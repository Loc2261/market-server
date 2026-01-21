using System.ComponentModel.DataAnnotations;

namespace MarketService.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(255)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; } // Giới thiệu bản thân

        [StringLength(500)]
        public string? CoverImageUrl { get; set; } // Ảnh bìa profile

        public int FollowersCount { get; set; } = 0; // Cache count
        public int FollowingCount { get; set; } = 0; // Cache count

        public DateTime? LastActive { get; set; } // Lần hoạt động cuối

        public string Role { get; set; } = "User"; // User, Admin

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Conversation> ConversationsAsUser1 { get; set; } = new List<Conversation>();
        public virtual ICollection<Conversation> ConversationsAsUser2 { get; set; } = new List<Conversation>();
        
        // Social features navigation
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();
        public virtual ICollection<UserVerification> Verifications { get; set; } = new List<UserVerification>();
        public virtual SellerScore? SellerScore { get; set; }
        public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
    }
}
