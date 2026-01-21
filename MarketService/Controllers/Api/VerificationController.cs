using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;

        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        // POST api/verification/request
        [Authorize]
        [HttpPost("request")]
        public async Task<IActionResult> RequestVerification([FromBody] RequestVerificationDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = await _verificationService.RequestVerificationAsync(userId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/verification/my
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyVerifications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var verifications = await _verificationService.GetUserVerificationsAsync(userId);
            return Ok(verifications);
        }

        // GET api/verification/level
        [Authorize]
        [HttpGet("level")]
        public async Task<IActionResult> GetMyLevel()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var level = await _verificationService.GetVerificationLevelAsync(userId);
            return Ok(new { level });
        }

        // GET api/verification/pending (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingVerifications()
        {
            var verifications = await _verificationService.GetPendingVerificationsAsync();
            return Ok(verifications);
        }

        // POST api/verification/approve/{id} (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveVerification(int id)
        {
            var success = await _verificationService.ApproveVerificationAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Xác thực đã được duyệt" });
        }

        // POST api/verification/reject/{id} (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectVerification(int id, [FromBody] string reason)
        {
            var success = await _verificationService.RejectVerificationAsync(id, reason);
            if (!success) return NotFound();
            return Ok(new { message = "Xác thực đã bị từ chối" });
        }
    }
}
