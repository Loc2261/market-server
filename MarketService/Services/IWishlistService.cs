using MarketService.Models;
using MarketService.DTOs;

namespace MarketService.Services
{
    public interface IWishlistService
    {
        Task<bool> AddToWishlistAsync(int userId, int productId);
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);
        Task<PagedResult<Wishlist>> GetUserWishlistAsync(int userId, int page = 1, int pageSize = 10);
        Task<bool> IsInWishlistAsync(int userId, int productId);
        Task<int> GetWishlistCountAsync(int userId);
    }
}
