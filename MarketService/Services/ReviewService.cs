using System.Collections.Generic;
using System.Threading.Tasks;
using MarketService.Data;
using MarketService.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketService.Services
{
    public class ReviewService : IReviewService
    {
        private readonly MarketDbContext _context;
        private readonly ISellerScoreService _sellerScoreService;

        public ReviewService(MarketDbContext context, ISellerScoreService sellerScoreService)
        {
            _context = context;
            _sellerScoreService = sellerScoreService;
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Cập nhật điểm người bán
            await _sellerScoreService.CalculateScoreAsync(review.SellerId);

            return review;
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetSellerReviewsAsync(int sellerId)
        {
            return await _context.Reviews
                .Where(r => r.SellerId == sellerId)
                .Include(r => r.Reviewer)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> CalculateAverageRatingAsync(int sellerId)
        {
            var reviews = await _context.Reviews.Where(r => r.SellerId == sellerId).ToListAsync();
            if (!reviews.Any()) return 0;
            return (decimal)reviews.Average(r => r.Rating);
        }
    }
}
