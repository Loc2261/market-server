using MarketService.DTOs;

namespace MarketService.Services
{
    public interface ISellerScoreService
    {
        // Tính điểm
        Task<SellerScoreDTO> CalculateScoreAsync(int userId);
        Task UpdateScoreAfterSaleAsync(int sellerId, bool isCompleted);
        Task RecalculateAllScoresAsync(); // Background job
        
        // Lấy thông tin
        Task<SellerScoreDTO?> GetSellerScoreAsync(int userId);
        Task<List<string>> GetBadgesAsync(int userId);
        
        // Leaderboard
        Task<List<SellerScoreDTO>> GetTopSellersAsync(int count);
    }
}
