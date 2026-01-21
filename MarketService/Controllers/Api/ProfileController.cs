using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services;
using MarketService.DTOs;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET api/profile/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = await _profileService.GetUserProfileAsync(userId);
            
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        // GET api/profile/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            int? viewerId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                viewerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            var profile = await _profileService.GetPublicProfileAsync(userId, viewerId);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        // PUT api/profile/me
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _profileService.UpdateProfileAsync(userId, dto);
            
            if (!success) return NotFound();
            return Ok(new { message = "Cập nhật profile thành công" });
        }
    }
}
