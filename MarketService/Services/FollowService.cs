using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services
{
    public class FollowService : IFollowService
    {
        private readonly MarketDbContext _context;
        private readonly IVerificationService _verificationService;

        public FollowService(MarketDbContext context, IVerificationService verificationService)
        {
            _context = context;
            _verificationService = verificationService;
        }

        public async Task<bool> FollowAsync(int followerId, int followingId)
        {
            // Không thể tự follow mình
            if (followerId == followingId) return false;

            // Kiểm tra đã follow chưa
            var existing = await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existing) return false;

            // Tạo follow mới
            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Follows.Add(follow);

            // Update cached counts
            var follower = await _context.Users.FindAsync(followerId);
            var following = await _context.Users.FindAsync(followingId);

            if (follower != null) follower.FollowingCount++;
            if (following != null) following.FollowersCount++;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowAsync(int followerId, int followingId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (follow == null) return false;

            _context.Follows.Remove(follow);

            // Update cached counts
            var follower = await _context.Users.FindAsync(followerId);
            var following = await _context.Users.FindAsync(followingId);

            if (follower != null && follower.FollowingCount > 0) follower.FollowingCount--;
            if (following != null && following.FollowersCount > 0) following.FollowersCount--;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<List<UserProfileDTO>> GetFollowersAsync(int userId, int page, int pageSize)
        {
            var followers = await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Include(f => f.Follower)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower)
                .ToListAsync();

            var dtos = new List<UserProfileDTO>();
            foreach (var user in followers)
            {
                dtos.Add(await MapToProfileDTO(user, null));
            }

            return dtos;
        }

        public async Task<List<UserProfileDTO>> GetFollowingAsync(int userId, int page, int pageSize)
        {
            var following = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Following)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Following)
                .ToListAsync();

            var dtos = new List<UserProfileDTO>();
            foreach (var user in following)
            {
                dtos.Add(await MapToProfileDTO(user, null));
            }

            return dtos;
        }

        public async Task<FollowStatsDTO> GetFollowStatsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new FollowStatsDTO();

            return new FollowStatsDTO
            {
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount
            };
        }

        public async Task SyncFollowCountsAsync()
        {
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                var followersCount = await _context.Follows.CountAsync(f => f.FollowingId == user.Id);
                var followingCount = await _context.Follows.CountAsync(f => f.FollowerId == user.Id);

                user.FollowersCount = followersCount;
                user.FollowingCount = followingCount;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<UserProfileDTO> MapToProfileDTO(User user, int? viewerId)
        {
            var verificationLevel = await _verificationService.GetVerificationLevelAsync(user.Id);
            var productCount = await _context.Products.CountAsync(p => p.SellerId == user.Id);
            var postCount = await _context.Posts.CountAsync(p => p.AuthorId == user.Id);

            return new UserProfileDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                CoverImageUrl = user.CoverImageUrl,
                FollowersCount = user.FollowersCount,
                FollowingCount = user.FollowingCount,
                ProductsCount = productCount,
                PostsCount = postCount,
                VerificationLevel = verificationLevel,
                VerifiedTypes = new List<string>(), // TODO: Get from verifications
                IsFollowing = viewerId.HasValue && await IsFollowingAsync(viewerId.Value, user.Id),
                IsFollowedBy = viewerId.HasValue && await IsFollowingAsync(user.Id, viewerId.Value),
                LastActive = user.LastActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
