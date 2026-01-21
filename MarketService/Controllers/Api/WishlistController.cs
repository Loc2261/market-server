using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketService.Services;
using System.Security.Claims;

namespace MarketService.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // GET api/wishlist
        [HttpGet]
        public async Task<IActionResult> GetWishlist([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _wishlistService.GetUserWishlistAsync(userId, page, pageSize);
            return Ok(result);
        }

        // POST api/wishlist/{productId}
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _wishlistService.AddToWishlistAsync(userId, productId);

            if (!success)
            {
                return BadRequest(new { message = "Sản phẩm đã có trong wishlist" });
            }

            return Ok(new { message = "Đã thêm vào wishlist" });
        }

        // DELETE api/wishlist/{productId}
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong wishlist" });
            }

            return Ok(new { message = "Đã xóa khỏi wishlist" });
        }

        // GET api/wishlist/count
        [HttpGet("count")]
        public async Task<IActionResult> GetWishlistCount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var count = await _wishlistService.GetWishlistCountAsync(userId);
            return Ok(new { count });
        }

        // GET api/wishlist/check/{productId}
        [HttpGet("check/{productId}")]
        public async Task<IActionResult> CheckWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);
            return Ok(new { isInWishlist });
        }
    }
}
