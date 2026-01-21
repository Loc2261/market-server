using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        // POST api/follow/{userId}
        [Authorize]
        [HttpPost("{userId}")]
        public async Task<IActionResult> FollowUser(int userId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            if (currentUserId == userId)
            {
                return BadRequest(new { message = "Không thể tự follow mình" });
            }

            var success = await _followService.FollowAsync(currentUserId, userId);
            
            if (!success)
            {
                return BadRequest(new { message = "Bạn đã follow người dùng này rồi" });
            }

            return Ok(new { message = "Đã follow thành công" });
        }

        // DELETE api/follow/{userId}
        [Authorize]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> UnfollowUser(int userId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _followService.UnfollowAsync(currentUserId, userId);
            
            if (!success)
            {
                return BadRequest(new { message = "Bạn chưa follow người dùng này" });
            }

            return Ok(new { message = "Đã unfollow thành công" });
        }

        // GET api/follow/check/{userId}
        [Authorize]
        [HttpGet("check/{userId}")]
        public async Task<IActionResult> CheckFollowing(int userId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isFollowing = await _followService.IsFollowingAsync(currentUserId, userId);
            return Ok(new { isFollowing });
        }

        // GET api/follow/followers/{userId}?page=1&pageSize=20
        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var followers = await _followService.GetFollowersAsync(userId, page, pageSize);
            return Ok(followers);
        }

        // GET api/follow/following/{userId}?page=1&pageSize=20
        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var following = await _followService.GetFollowingAsync(userId, page, pageSize);
            return Ok(following);
        }

        // GET api/follow/stats/{userId}
        [HttpGet("stats/{userId}")]
        public async Task<IActionResult> GetFollowStats(int userId)
        {
            var stats = await _followService.GetFollowStatsAsync(userId);
            return Ok(stats);
        }
    }
}
