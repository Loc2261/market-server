using System.Collections.Generic;
using System.Threading.Tasks;
using MarketService.Models;

namespace MarketService.Services
{
    public interface IReviewService
    {
        Task<Review> CreateReviewAsync(Review review);
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);
        Task<IEnumerable<Review>> GetSellerReviewsAsync(int sellerId);
        Task<decimal> CalculateAverageRatingAsync(int sellerId);
    }
}
