using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly MarketDbContext _context;

        public VerificationService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<VerificationResponseDTO> RequestVerificationAsync(int userId, RequestVerificationDTO dto)
        {
            // Kiểm tra xem đã có yêu cầu verify loại này chưa
            var existing = await _context.UserVerifications
                .FirstOrDefaultAsync(v => v.UserId == userId && v.Type == dto.Type);

            if (existing != null && existing.Status == VerificationStatus.Verified)
            {
                throw new InvalidOperationException($"Bạn đã được xác thực {dto.Type}");
            }

            if (existing != null && existing.Status == VerificationStatus.Pending)
            {
                throw new InvalidOperationException($"Yêu cầu xác thực {dto.Type} đang chờ duyệt");
            }

            // Tạo yêu cầu mới hoặc update yêu cầu bị reject
            var verification = existing ?? new UserVerification
            {
                UserId = userId,
                Type = dto.Type
            };

            verification.Status = VerificationStatus.Pending;
            verification.DocumentUrl = dto.DocumentUrl;
            verification.RejectionReason = null;
            verification.CreatedAt = DateTime.UtcNow;

            if (existing == null)
            {
                _context.UserVerifications.Add(verification);
            }

            await _context.SaveChangesAsync();

            return MapToDTO(verification);
        }

        public async Task<bool> ApproveVerificationAsync(int verificationId)
        {
            var verification = await _context.UserVerifications.FindAsync(verificationId);
            if (verification == null) return false;

            verification.Status = VerificationStatus.Verified;
            verification.VerifiedAt = DateTime.UtcNow;
            verification.RejectionReason = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectVerificationAsync(int verificationId, string reason)
        {
            var verification = await _context.UserVerifications.FindAsync(verificationId);
            if (verification == null) return false;

            verification.Status = VerificationStatus.Rejected;
            verification.RejectionReason = reason;
            verification.VerifiedAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<VerificationResponseDTO>> GetUserVerificationsAsync(int userId)
        {
            return await _context.UserVerifications
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => MapToDTO(v))
                .ToListAsync();
        }

        public async Task<int> GetVerificationLevelAsync(int userId)
        {
            var verifiedTypes = await _context.UserVerifications
                .Where(v => v.UserId == userId && v.Status == VerificationStatus.Verified)
                .Select(v => v.Type)
                .ToListAsync();

            // Level 0: Chưa verify gì
            if (verifiedTypes.Count == 0) return 0;

            // Level 1: Phone
            if (verifiedTypes.Contains(VerificationType.Phone) && verifiedTypes.Count == 1) return 1;

            // Level 2: Phone + Email
            if (verifiedTypes.Contains(VerificationType.Phone) && 
                verifiedTypes.Contains(VerificationType.Email) && 
                verifiedTypes.Count == 2) return 2;

            // Level 3: Phone + Email + ID
            if (verifiedTypes.Contains(VerificationType.Phone) && 
                verifiedTypes.Contains(VerificationType.Email) &&
                verifiedTypes.Contains(VerificationType.ID)) return 3;

            // Level 4: All + Business
            if (verifiedTypes.Contains(VerificationType.Business)) return 4;

            return verifiedTypes.Count; // Fallback
        }

        public async Task<bool> IsVerifiedAsync(int userId, VerificationType type)
        {
            return await _context.UserVerifications
                .AnyAsync(v => v.UserId == userId && v.Type == type && v.Status == VerificationStatus.Verified);
        }

        public async Task<List<VerificationResponseDTO>> GetPendingVerificationsAsync()
        {
            return await _context.UserVerifications
                .Where(v => v.Status == VerificationStatus.Pending)
                .OrderBy(v => v.CreatedAt)
                .Select(v => MapToDTO(v))
                .ToListAsync();
        }

        private static VerificationResponseDTO MapToDTO(UserVerification v)
        {
            return new VerificationResponseDTO
            {
                Id = v.Id,
                UserId = v.UserId,
                Type = v.Type.ToString(),
                Status = v.Status.ToString(),
                DocumentUrl = v.DocumentUrl,
                RejectionReason = v.RejectionReason,
                VerifiedAt = v.VerifiedAt,
                CreatedAt = v.CreatedAt
            };
        }
    }
}
