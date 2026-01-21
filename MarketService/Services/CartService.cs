using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketService.Services
{
    public class CartService : ICartService
    {
        private readonly MarketDbContext _context;

        public CartService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItemResponseDTO>> GetCartItemsAsync(int userId)
        {
            return await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .OrderByDescending(ci => ci.CreatedAt)
                .Select(ci => new CartItemResponseDTO
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    Title = ci.Product.Title,
                    Price = ci.Product.Price,
                    ImageUrl = ci.Product.ImageUrl,
                    Quantity = ci.Quantity
                })
                .ToListAsync();
        }

        public async Task<CartItemResponseDTO> AddToCartAsync(int userId, AddToCartDTO dto)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);

            if (cartItem != null)
            {
                cartItem.Quantity += dto.Quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var product = await _context.Products.FindAsync(dto.ProductId);
            return new CartItemResponseDTO
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                Title = product?.Title ?? "",
                Price = product?.Price ?? 0,
                ImageUrl = product?.ImageUrl,
                Quantity = cartItem.Quantity
            };
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var items = _context.CartItems.Where(ci => ci.UserId == userId);
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartCountAsync(int userId)
        {
            return await _context.CartItems.CountAsync(ci => ci.UserId == userId);
        }
    }
}
