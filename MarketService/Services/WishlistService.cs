using MarketService.Data;
using MarketService.Models;
using Microsoft.EntityFrameworkCore;
using MarketService.DTOs;
using MarketService.Helpers;

namespace MarketService.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly MarketDbContext _context;

        public WishlistService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddToWishlistAsync(int userId, int productId)
        {
            // Check if already in wishlist
            var exists = await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            if (exists)
            {
                return false; // Already in wishlist
            }

            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
        {
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (wishlist == null) return false;

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<WishlistResponseDTO>> GetUserWishlistAsync(int userId, int page = 1, int pageSize = 12)
        {
            var query = _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p.Seller)
                .Where(w => w.UserId == userId && w.Product != null) // Add null check for product
                .OrderByDescending(w => w.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, w => new WishlistResponseDTO
            {
                Id = w.Id,
                ProductId = w.ProductId,
                Title = w.Product.Title,
                ImageUrl = w.Product.ImageUrl,
                Price = w.Product.Price,
                Category = w.Product.Category,
                SellerName = w.Product.Seller?.FullName ?? w.Product.Seller?.Username ?? "Unknown",
                AddedAt = w.CreatedAt
            });
        }

        public async Task<bool> IsInWishlistAsync(int userId, int productId)
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task<int> GetWishlistCountAsync(int userId)
        {
            return await _context.Wishlists
                .CountAsync(w => w.UserId == userId);
        }
    }
}
