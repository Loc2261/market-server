using System.Collections.Generic;
using System.Threading.Tasks;
using MarketService.Models;

namespace MarketService.Services
{
    public interface ISocialService
    {
        // Likes
        Task<bool> LikePostAsync(int userId, int postId);
        Task<bool> UnlikePostAsync(int userId, int postId);
        Task<int> GetPostLikesCountAsync(int postId);
        Task<bool> IsPostLikedByUserAsync(int userId, int postId);

        // Comments
        Task<PostComment> AddCommentAsync(int userId, int postId, string content, int? parentId = null);
        Task<IEnumerable<PostComment>> GetPostCommentsAsync(int postId);
        Task<bool> DeleteCommentAsync(int userId, int commentId);
    }
}
