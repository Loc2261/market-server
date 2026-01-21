using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services
{
    public class ProfileService : IProfileService
    {
        private readonly MarketDbContext _context;
        private readonly IVerificationService _verificationService;
        private readonly ISellerScoreService _sellerScoreService;
        private readonly IFollowService _followService;

        public ProfileService(
            MarketDbContext context,
            IVerificationService verificationService,
            ISellerScoreService sellerScoreService,
            IFollowService followService)
        {
            _context = context;
            _verificationService = verificationService;
            _sellerScoreService = sellerScoreService;
            _followService = followService;
        }

        public async Task<UserProfileDTO?> GetUserProfileAsync(int userId, int? viewerId = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            // Get verification level and types
            var verificationLevel = await _verificationService.GetVerificationLevelAsync(userId);
            var verifications = await _verificationService.GetUserVerificationsAsync(userId);
            var verifiedTypes = verifications
                .Where(v => v.Status == "Verified")
                .Select(v => v.Type)
                .ToList();

            // Get seller score
            var sellerScore = await _sellerScoreService.GetSellerScoreAsync(userId);

            // Get follow stats
            var followStats = await _followService.GetFollowStatsAsync(userId);

            // Get products and posts count
            var productsCount = await _context.Products.CountAsync(p => p.SellerId == userId);
            var postsCount = await _context.Posts.CountAsync(p => p.AuthorId == userId);

            // Check follow relationship if viewer is provided
            bool isFollowing = false;
            bool isFollowedBy = false;
            if (viewerId.HasValue && viewerId.Value != userId)
            {
                isFollowing = await _followService.IsFollowingAsync(viewerId.Value, userId);
                isFollowedBy = await _followService.IsFollowingAsync(userId, viewerId.Value);
            }

            return new UserProfileDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                CoverImageUrl = user.CoverImageUrl,
                Role = user.Role,
                FollowersCount = followStats.FollowersCount,
                FollowingCount = followStats.FollowingCount,
                ProductsCount = productsCount,
                PostsCount = postsCount,
                VerificationLevel = verificationLevel,
                VerifiedTypes = verifiedTypes,
                SellerScore = sellerScore,
                IsFollowing = isFollowing,
                IsFollowedBy = isFollowedBy,
                LastActive = user.LastActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Update fields
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.Bio != null) user.Bio = dto.Bio;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
            if (dto.CoverImageUrl != null) user.CoverImageUrl = dto.CoverImageUrl;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserProfileDTO?> GetPublicProfileAsync(int userId, int? viewerId = null)
        {
            // For now, same as GetUserProfileAsync
            // Can add privacy logic later (e.g., hide some info if not following)
            return await GetUserProfileAsync(userId, viewerId);
        }
    }
}
