using MarketService.DTOs;

namespace MarketService.Services
{
    public interface IProfileService
    {
        // Get comprehensive user profile
        Task<UserProfileDTO?> GetUserProfileAsync(int userId, int? viewerId = null);
        
        // Update profile
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDTO dto);
        
        // Get profile with privacy check
        Task<UserProfileDTO?> GetPublicProfileAsync(int userId, int? viewerId = null);
    }
}
