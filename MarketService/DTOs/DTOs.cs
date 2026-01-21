using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MarketService.DTOs
{
    // ============ AUTH DTOs ============
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? FullName { get; set; }
    }

    public class LoginDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập hoặc Email là bắt buộc")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public UserResponseDTO? User { get; set; }
    }

    // ============ USER DTOs ============
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserDTO
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    // ============ PRODUCT DTOs ============
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? ImageUrls { get; set; } = new();
        public IFormFile? ImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
        public string? Location { get; set; }
    }

    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string Status { get; set; } = "Available";
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string SellerUsername { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============ POST DTOs ============
    public class CreatePostDTO
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
    }


    public class PostResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int SharesCount { get; set; }
        public bool IsLikedByUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============ CHAT DTOs ============
    public class SendMessageDTO
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
    }

    public class MessageResponseDTO
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ConversationResponseDTO
    {
        public int Id { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string OtherUserUsername { get; set; } = string.Empty;
        public string? OtherUserAvatar { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    // ============ CART DTOs ============
    public class CartItemResponseDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
    }

    public class AddToCartDTO
    {
        [Required]
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    // ============ SHIPPING/ORDER DTOs ============
    public class CreateOrderDTO
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public string Provider { get; set; } = "GHN";
        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
    }

    public class ShippingOrderResponseDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class ShippingTrackingInfo
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public DateTime? ExpectedDelivery { get; set; }
        public List<TrackingLogDTO> Logs { get; set; } = new();
    }

    public class TrackingLogDTO
    {
        public DateTime Time { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AdminDashboardStatsDTO
    {
        public int TotalUsers { get; set; }
        public int ActiveProducts { get; set; }
        public int SuccessfulOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ActivityResponseDTO> RecentActivities { get; set; } = new();
    }

    public class ActivityResponseDTO
    {
        public string Type { get; set; } = string.Empty; // User, Product, Order
        public string Detail { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ============ CATEGORY DTOs ============
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Slug { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
    
    public class SetRoleDTO
    {
        public string Role { get; set; } = "User";
    }
}
