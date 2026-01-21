using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services
{
    public class SellerScoreService : ISellerScoreService
    {
        private readonly MarketDbContext _context;

        public SellerScoreService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<SellerScoreDTO> CalculateScoreAsync(int userId)
        {
            var sellerScore = await _context.SellerScores.FirstOrDefaultAsync(s => s.UserId == userId);
            
            if (sellerScore == null)
            {
                sellerScore = new SellerScore { UserId = userId };
                _context.SellerScores.Add(sellerScore);
            }

            // Tính các metrics từ data thực tế (giả định có bảng Orders)
            // Hiện tại dùng Products để demo
            var products = await _context.Products
                .Where(p => p.SellerId == userId)
                .ToListAsync();

            sellerScore.TotalSales = products.Count(p => p.Status == ProductStatus.Sold);
            sellerScore.CompletedOrders = sellerScore.TotalSales; // Simplification
            sellerScore.CancelledOrders = 0; // TODO: Get from orders table

            // Tính completion rate
            var totalOrders = sellerScore.CompletedOrders + sellerScore.CancelledOrders;
            sellerScore.CompletionRate = totalOrders > 0 ? 
                (decimal)sellerScore.CompletedOrders / totalOrders * 100 : 0;

            // TODO: Tính AverageResponseTime từ messages
            sellerScore.AverageResponseTime = 10; // Giả định 10 phút

            // Tính ratings từ reviews table
            var reviews = await _context.Reviews
                .Where(r => r.SellerId == userId)
                .ToListAsync();

            sellerScore.TotalReviews = reviews.Count;
            sellerScore.AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

            // Tính Overall Score
            sellerScore.OverallScore = CalculateOverallScore(sellerScore);

            // Assign badges
            var badges = AssignBadges(sellerScore);
            sellerScore.Badges = JsonSerializer.Serialize(badges);
            
            sellerScore.LastCalculated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToDTO(sellerScore, badges);
        }

        private decimal CalculateOverallScore(SellerScore score)
        {
            // Công thức: (CompletionRate% * 0.3) + (AvgRating * 0.4) + (ResponseScore *  0.2) + (SalesVolume * 0.1)
            decimal completionScore = (score.CompletionRate / 100) * 0.3m;
            decimal ratingScore = (score.AverageRating / 5) * 0.4m;
            
            // Response score: < 5 phút = 1.0, < 15 phút = 0.8, < 30 phút = 0.5, else = 0.2
            decimal responseScore = score.AverageResponseTime.HasValue ?
                (score.AverageResponseTime.Value < 5 ? 1.0m :
                 score.AverageResponseTime.Value < 15 ? 0.8m :
                 score.AverageResponseTime.Value < 30 ? 0.5m : 0.2m) * 0.2m : 0;

            // Sales volume score: normalize to 0-1 (100 sales = 1.0)
            decimal salesScore = Math.Min(score.TotalSales / 100m, 1.0m) * 0.1m;

            decimal overall = (completionScore + ratingScore + responseScore + salesScore) * 5m;
            return Math.Round(Math.Min(overall, 5.00m), 2);
        }

        private List<string> AssignBadges(SellerScore score)
        {
            var badges = new List<string>();

            if (score.TotalSales >= 100)
                badges.Add("TopSeller");

            if (score.AverageResponseTime.HasValue && score.AverageResponseTime.Value < 5)
                badges.Add("FastResponder");

            if (score.CompletionRate >= 95)
                badges.Add("TrustedSeller");

            if (score.AverageRating >= 4.8m && score.TotalReviews >= 10)
                badges.Add("5StarSeller");

            return badges;
        }

        public async Task UpdateScoreAfterSaleAsync(int sellerId, bool isCompleted)
        {
            var score = await _context.SellerScores.FirstOrDefaultAsync(s => s.UserId == sellerId);
            if (score == null) return;

            if (isCompleted)
            {
                score.TotalSales++;
                score.CompletedOrders++;
            }
            else
            {
                score.CancelledOrders++;
            }

            // Recalculate
            await CalculateScoreAsync(sellerId);
        }

        public async Task RecalculateAllScoresAsync()
        {
            var sellers = await _context.Users
                .Where(u => u.Products.Any())
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var sellerId in sellers)
            {
                await CalculateScoreAsync(sellerId);
            }
        }

        public async Task<SellerScoreDTO?> GetSellerScoreAsync(int userId)
        {
            var score = await _context.SellerScores.FirstOrDefaultAsync(s => s.UserId == userId);
            if (score == null) return null;

            var badges = string.IsNullOrEmpty(score.Badges) ? 
                new List<string>() : 
                JsonSerializer.Deserialize<List<string>>(score.Badges) ?? new List<string>();

            return MapToDTO(score, badges);
        }

        public async Task<List<string>> GetBadgesAsync(int userId)
        {
            var score = await _context.SellerScores.FirstOrDefaultAsync(s => s.UserId == userId);
            if (score == null || string.IsNullOrEmpty(score.Badges)) 
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(score.Badges) ?? new List<string>();
        }

        public async Task<List<SellerScoreDTO>> GetTopSellersAsync(int count)
        {
            var scores = await _context.SellerScores
                .OrderByDescending(s => s.OverallScore)
                .Take(count)
                .ToListAsync();

            // Map to DTOs after materialization (not in expression tree)
            return scores.Select(s =>
            {
                var badges = string.IsNullOrEmpty(s.Badges) ?
                    new List<string>() :
                    JsonSerializer.Deserialize<List<string>>(s.Badges) ?? new List<string>();
                return MapToDTO(s, badges);
            }).ToList();
        }

        private static SellerScoreDTO MapToDTO(SellerScore score, List<string> badges)
        {
            return new SellerScoreDTO
            {
                UserId = score.UserId,
                OverallScore = score.OverallScore,
                TotalSales = score.TotalSales,
                CompletedOrders = score.CompletedOrders,
                CompletionRate = score.CompletionRate,
                AverageRating = score.AverageRating,
                AverageResponseTime = score.AverageResponseTime,
                Badges = badges,
                LastCalculated = score.LastCalculated
            };
        }
    }
}
