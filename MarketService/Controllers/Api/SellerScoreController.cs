using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerScoreController : ControllerBase
    {
        private readonly ISellerScoreService _sellerScoreService;

        public SellerScoreController(ISellerScoreService sellerScoreService)
        {
            _sellerScoreService = sellerScoreService;
        }

        // GET api/sellerscore/my
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyScore()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var score = await _sellerScoreService.GetSellerScoreAsync(userId);
            
            if (score == null)
            {
                return Ok(new { message = "Chưa có điểm uy tín", score = (object?)null });
            }
            
            return Ok(score);
        }

        // GET api/sellerscore/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserScore(int userId)
        {
            var score = await _sellerScoreService.GetSellerScoreAsync(userId);
            if (score == null) return NotFound();
            return Ok(score);
        }

        // GET api/sellerscore/badges/{userId}
        [HttpGet("badges/{userId}")]
        public async Task<IActionResult> GetBadges(int userId)
        {
            var badges = await _sellerScoreService.GetBadgesAsync(userId);
            return Ok(badges);
        }

        // GET api/sellerscore/leaderboard?count=10
        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int count = 10)
        {
            var topSellers = await _sellerScoreService.GetTopSellersAsync(count);
            return Ok(topSellers);
        }

        // POST api/sellerscore/recalculate (My score only)
        [Authorize]
        [HttpPost("recalculate")]
        public async Task<IActionResult> RecalculateMyScore()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var score = await _sellerScoreService.CalculateScoreAsync(userId);
            return Ok(score);
        }
    }
}
