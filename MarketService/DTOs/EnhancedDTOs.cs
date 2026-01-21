using System.ComponentModel.DataAnnotations;
using MarketService.Models;
using Microsoft.AspNetCore.Http;

namespace MarketService.DTOs
{
    // ============ VERIFICATION DTOs ============
    public class RequestVerificationDTO
    {
        [Required]
        public VerificationType Type { get; set; }

        [StringLength(500)]
        public string? DocumentUrl { get; set; }
    }

    public class VerificationResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? DocumentUrl { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============ SELLER SCORE DTOs ============
    public class SellerScoreDTO
    {
        public int UserId { get; set; }
        public decimal OverallScore { get; set; } // 0-5
        public int TotalSales { get; set; }
        public int CompletedOrders { get; set; }
        public decimal CompletionRate { get; set; } // %
        public decimal AverageRating { get; set; }
        public decimal? AverageResponseTime { get; set; } // Corrected type to match Model
        public List<string> Badges { get; set; } = new();
        public DateTime LastCalculated { get; set; }
    }

    // ============ FOLLOW DTOs ============
    public class FollowStatsDTO
    {
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }

    // ============ PROFILE DTOs ============
    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Role { get; set; } = "User";
        
        // Stats
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int ProductsCount { get; set; }
        public int PostsCount { get; set; }
        
        // Verification & Score
        public int VerificationLevel { get; set; } // 0-4
        public List<string> VerifiedTypes { get; set; } = new();
        public SellerScoreDTO? SellerScore { get; set; }
        
        // Social
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; }
        
        public DateTime? LastActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }

        [StringLength(255)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public IFormFile? CoverFile { get; set; }
    }

    // ============ SHIPPING DTOs ============
    public class CreateAddressDTO
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Province { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        public string? Ward { get; set; }

        [Required]
        public string AddressLine { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }

    public class ShippingFeeDTO
    {
        public string Provider { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
    }

    public class CreateShippingOrderDTO
    {
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public int PickupAddressId { get; set; }
        public int DeliveryAddressId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
    }

    // ============ AI SUGGESTION DTOs ============

    public class CategorySuggestionDTO
    {
        public string Category { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}
