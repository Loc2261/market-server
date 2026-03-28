using MarketService.Models;
using MarketService.DTOs;

namespace MarketService.Services
{
    public interface IWishlistService
    {
        Task<bool> AddToWishlistAsync(int userId, int productId);
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);
        Task<PagedResult<WishlistResponseDTO>> GetUserWishlistAsync(int userId, int page = 1, int pageSize = 12);
        Task<bool> IsInWishlistAsync(int userId, int productId);
        Task<int> GetWishlistCountAsync(int userId);
    }
}
