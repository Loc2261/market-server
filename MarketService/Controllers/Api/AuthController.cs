using Microsoft.AspNetCore.Mvc;
using MarketService.DTOs;
using MarketService.Services;

namespace MarketService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Set cookie for MVC views
            if (result.Token != null)
            {
                Response.Cookies.Append("auth_token", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Set cookie for MVC views
            if (result.Token != null)
            {
                Response.Cookies.Append("auth_token", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<AuthResponseDTO>> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.RequestPasswordResetAsync(dto);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponseDTO>> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.ResetPasswordAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token");
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}
