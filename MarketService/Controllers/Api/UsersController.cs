using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MarketService.DTOs;
using MarketService.Services;

namespace MarketService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private bool IsAdmin() => User.IsInRole("Admin");

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<UserResponseDTO>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userService.GetAllAsync(search, page, pageSize);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDTO>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(user);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDTO>> GetCurrentUser()
        {
            var user = await _userService.GetByIdAsync(GetUserId());
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(user);
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserResponseDTO>> UpdateCurrentUser([FromBody] UpdateUserDTO dto)
        {
            var user = await _userService.UpdateAsync(GetUserId(), dto);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null && user.Username == "admin")
            {
                return BadRequest(new { message = "Không thể xóa tài khoản quản trị viên gốc của hệ thống." });
            }

            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(new { message = "Đã xóa người dùng" });
        }

        [HttpPost("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null && user.Username == "admin")
            {
                return BadRequest(new { message = "Không thể thay đổi trạng thái của quản trị viên gốc." });
            }

            var result = await _userService.ToggleActiveAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(new { message = "Đã thay đổi trạng thái" });
        }

        [HttpPost("{id}/set-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetRole(int id, [FromBody] SetRoleDTO dto)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null && user.Username == "admin")
            {
                return BadRequest(new { message = "Không thể thay đổi vai trò của quản trị viên gốc." });
            }

            var result = await _userService.SetRoleAsync(id, dto.Role);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(new { message = "Đã cập nhật vai trò" });
        }
    }

    public class SetRoleDTO
    {
        public string Role { get; set; } = "User";
    }
}
