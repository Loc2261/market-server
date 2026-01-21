using System.Collections.Generic;
using System.Threading.Tasks;
using MarketService.DTOs;

namespace MarketService.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemResponseDTO>> GetCartItemsAsync(int userId);
        Task<CartItemResponseDTO> AddToCartAsync(int userId, AddToCartDTO dto);
        Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
        Task<bool> ClearCartAsync(int userId);
        Task<int> GetCartCountAsync(int userId);
    }
}
