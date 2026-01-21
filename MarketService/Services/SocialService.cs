using System.Collections.Generic;
using System.Threading.Tasks;
using MarketService.Data;
using MarketService.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketService.Services
{
    public class SocialService : ISocialService
    {
        private readonly MarketDbContext _context;
        private readonly INotificationService _notificationService;

        public SocialService(MarketDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> LikePostAsync(int userId, int postId)
        {
            if (await _context.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId))
                return true;

            var like = new PostLike { PostId = postId, UserId = userId };
            _context.PostLikes.Add(like);
            
            var post = await _context.Posts.FindAsync(postId);
            if (post != null && post.AuthorId != userId)
            {
                var user = await _context.Users.FindAsync(userId);
                await _notificationService.CreateNotificationAsync(
                    post.AuthorId, 
                    "Lượt thích mới", 
                    $"{user?.Username} đã thích bài viết của bạn: {post.Title}",
                    $"/Post/Details/{postId}");
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikePostAsync(int userId, int postId)
        {
            var like = await _context.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null) return true;

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetPostLikesCountAsync(int postId)
        {
            return await _context.PostLikes.CountAsync(l => l.PostId == postId);
        }

        public async Task<bool> IsPostLikedByUserAsync(int userId, int postId)
        {
            return await _context.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<PostComment> AddCommentAsync(int userId, int postId, string content, int? parentId = null)
        {
            var comment = new PostComment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                ParentId = parentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostComments.Add(comment);
            
            var post = await _context.Posts.FindAsync(postId);
            if (post != null && post.AuthorId != userId)
            {
                var user = await _context.Users.FindAsync(userId);
                await _notificationService.CreateNotificationAsync(
                    post.AuthorId, 
                    "Bình luận mới", 
                    $"{user?.Username} đã bình luận về bài viết của bạn.",
                    $"/Post/Details/{postId}");
            }

            await _context.SaveChangesAsync();
            
            // Explicitly load the User to return complete data
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();
            
            return comment;
        }

        public async Task<IEnumerable<PostComment>> GetPostCommentsAsync(int postId)
        {
            return await _context.PostComments
                .Where(c => c.PostId == postId && c.ParentId == null)
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteCommentAsync(int userId, int commentId)
        {
            var comment = await _context.PostComments.FindAsync(commentId);
            if (comment == null || comment.UserId != userId) return false;

            _context.PostComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
