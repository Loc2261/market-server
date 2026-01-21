using MarketService.DTOs;

namespace MarketService.Services
{
    public interface IFollowService
    {
        // Follow/Unfollow
        Task<bool> FollowAsync(int followerId, int followingId);
        Task<bool> UnfollowAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        
        // Danh sách
        Task<List<UserProfileDTO>> GetFollowersAsync(int userId, int page, int pageSize);
        Task<List<UserProfileDTO>> GetFollowingAsync(int userId, int page, int pageSize);
        
        // Thống kê
        Task<FollowStatsDTO> GetFollowStatsAsync(int userId);
        Task SyncFollowCountsAsync(); // Background job để sync cached counts
    }
}
