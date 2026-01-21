using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services
{
    public interface IVerificationService
    {
        // Yêu cầu xác thực
        Task<VerificationResponseDTO> RequestVerificationAsync(int userId, RequestVerificationDTO dto);
        
        // Admin duy ệt xác thực
        Task<bool> ApproveVerificationAsync(int verificationId);
        Task<bool> RejectVerificationAsync(int verificationId, string reason);
        
        // Kiểm tra trạng thái
        Task<List<VerificationResponseDTO>> GetUserVerificationsAsync(int userId);
        Task<int> GetVerificationLevelAsync(int userId); // Trả về cấp độ 0-4
        Task<bool> IsVerifiedAsync(int userId, VerificationType type);
        Task<List<VerificationResponseDTO>> GetPendingVerificationsAsync(); // Admin
    }
}
