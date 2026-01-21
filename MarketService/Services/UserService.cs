using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.Models;
using MarketService.DTOs;
using MarketService.Helpers;

namespace MarketService.Services
{
    public interface IUserService
    {
        Task<PagedResult<UserResponseDTO>> GetAllAsync(string? search = null, int page = 1, int pageSize = 10);
        Task<UserResponseDTO?> GetByIdAsync(int id);
        Task<UserResponseDTO?> UpdateAsync(int id, UpdateUserDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<bool> SetRoleAsync(int id, string role);
    }

    public class UserService : IUserService
    {
        private readonly MarketDbContext _context;

        public UserService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserResponseDTO>> GetAllAsync(string? search = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Username.Contains(search) || u.FullName.Contains(search) || u.Email.Contains(search));
            }

            return await query.OrderByDescending(u => u.CreatedAt)
                              .ToPagedResultAsync(page, pageSize, MapToResponse);
        }

        public async Task<UserResponseDTO?> GetByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponseDTO?> UpdateAsync(int id, UpdateUserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
            if (dto.Bio != null) user.Bio = dto.Bio;
            if (dto.CoverImageUrl != null) user.CoverImageUrl = dto.CoverImageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Username == "admin")
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Username == "admin")
            {
                return false;
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetRoleAsync(int id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Username == "admin")
            {
                return false;
            }

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private static UserResponseDTO MapToResponse(User u)
        {
            return new UserResponseDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                AvatarUrl = u.AvatarUrl,
                Bio = u.Bio,
                CoverImageUrl = u.CoverImageUrl,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            };
        }
    }
}
