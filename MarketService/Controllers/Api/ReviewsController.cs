using MarketService.Models;
using MarketService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var review = new Review
            {
                OrderId = request.OrderId,
                ReviewerId = userId,
                SellerId = request.SellerId,
                ProductId = request.ProductId,
                Rating = request.Rating,
                Comment = request.Comment
            };

            var result = await _reviewService.CreateReviewAsync(review);
            return Ok(result);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _reviewService.GetProductReviewsAsync(productId);
            return Ok(reviews);
        }

        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetSellerReviews(int sellerId)
        {
            var reviews = await _reviewService.GetSellerReviewsAsync(sellerId);
            return Ok(reviews);
        }
    }

    public class CreateReviewRequest
    {
        public int OrderId { get; set; }
        public int SellerId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
