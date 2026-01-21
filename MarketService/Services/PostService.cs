using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.Models;
using MarketService.DTOs;
using MarketService.Helpers;

namespace MarketService.Services
{
    public interface IPostService
    {
        Task<PagedResult<PostResponseDTO>> GetAllAsync(string? search = null, int currentUserId = 0, int page = 1, int pageSize = 10);
        Task<PostResponseDTO?> GetByIdAsync(int id, int currentUserId = 0);
        Task<PostResponseDTO> CreateAsync(CreatePostDTO dto, int authorId, List<string>? imageUrls = null);
        Task<PostResponseDTO?> UpdateAsync(int id, CreatePostDTO dto, int userId);
        Task<bool> DeleteAsync(int id, int userId, bool isAdmin);
        Task<PagedResult<PostResponseDTO>> GetByAuthorAsync(int authorId, int currentUserId = 0, int page = 1, int pageSize = 10);
        Task<bool> IncrementShareCountAsync(int postId);
    }

    public class PostService : IPostService
    {
        private readonly MarketDbContext _context;

        public PostService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PostResponseDTO>> GetAllAsync(string? search = null, int currentUserId = 0, int page = 1, int pageSize = 10)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.IsPublished);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            return await query.OrderByDescending(p => p.CreatedAt)
                              .ToPagedResultAsync(page, pageSize, p => MapToResponse(p, currentUserId));
        }

        public async Task<PostResponseDTO?> GetByIdAsync(int id, int currentUserId = 0)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            return post == null ? null : MapToResponse(post, currentUserId);
        }

        public async Task<PostResponseDTO> CreateAsync(CreatePostDTO dto, int authorId, List<string>? imageUrls = null)
        {
            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = imageUrls?.FirstOrDefault() ?? dto.ImageUrl,
                AuthorId = authorId,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            if (imageUrls != null && imageUrls.Any())
            {
                foreach (var url in imageUrls)
                {
                    post.Images.Add(new PostImage { ImageUrl = url });
                }
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            await _context.Entry(post).Reference(p => p.Author).LoadAsync();

            return MapToResponse(post);
        }

        public async Task<PostResponseDTO?> UpdateAsync(int id, CreatePostDTO dto, int userId)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null || post.AuthorId != userId)
            {
                return null;
            }

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.ImageUrl = dto.ImageUrl ?? post.ImageUrl;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(post, userId);
        }

        public async Task<bool> DeleteAsync(int id, int userId, bool isAdmin)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return false;
            }

            if (post.AuthorId != userId && !isAdmin)
            {
                return false;
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<PostResponseDTO>> GetByAuthorAsync(int authorId, int currentUserId = 0, int page = 1, int pageSize = 10)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, p => MapToResponse(p, currentUserId));
        }

        public async Task<bool> IncrementShareCountAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            post.SharesCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        private static PostResponseDTO MapToResponse(Post p, int currentUserId = 0)
        {
            return new PostResponseDTO
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                ImageUrls = (p.Images ?? new List<PostImage>()).Select(i => i.ImageUrl ?? "").ToList(),
                AuthorId = p.AuthorId,
                AuthorName = p.Author?.FullName ?? p.Author?.Username ?? "Unknown",
                AuthorAvatar = p.Author?.AvatarUrl,
                LikesCount = p.Likes?.Count ?? 0,
                CommentsCount = p.Comments?.Count ?? 0,
                SharesCount = p.SharesCount,
                IsLikedByUser = currentUserId > 0 && p.Likes != null && p.Likes.Any(l => l.UserId == currentUserId),
                CreatedAt = p.CreatedAt
            };
        }
    }
}
