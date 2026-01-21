using MarketService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly ISocialService _socialService;
        private readonly IPostService _postService;

        public CommunityController(ISocialService socialService, IPostService postService)
        {
            _socialService = socialService;
            _postService = postService;
        }

        [Authorize]
        [HttpPost("like/{postId}")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _socialService.LikePostAsync(userId, postId);
            return Ok();
        }

        [Authorize]
        [HttpDelete("unlike/{postId}")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _socialService.UnlikePostAsync(userId, postId);
            return Ok();
        }

        [HttpGet("likes-count/{postId}")]
        public async Task<IActionResult> GetLikesCount(int postId)
        {
            var count = await _socialService.GetPostLikesCountAsync(postId);
            return Ok(new { count });
        }

        [Authorize]
        [HttpPost("comment")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _socialService.AddCommentAsync(userId, request.PostId, request.Content, request.ParentId);
            return Ok(result);
        }

        [HttpGet("comments/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var result = await _socialService.GetPostCommentsAsync(postId);
            return Ok(result);
        }

        [HttpPost("share/{postId}")]
        public async Task<IActionResult> SharePost(int postId)
        {
            var result = await _postService.IncrementShareCountAsync(postId);
            if (!result) return NotFound();
            return Ok();
        }
    }

    public class AddCommentRequest
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
