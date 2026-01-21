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
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IAuthService _authService;

        public PostsController(IPostService postService, IAuthService authService)
        {
            _postService = postService;
            _authService = authService;
        }

        private int GetUserId()
        {
            // 1. Try claims first
            if (User.Identity?.IsAuthenticated == true)
            {
                return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            }

            // 2. Fallback: Manually check cookie
            if (Request.Cookies.TryGetValue("auth_token", out var token))
            {
                // Validate token manually
                var userId = _authService.ValidateToken(token);
                if (userId.HasValue) return userId.Value;
            }

            return 0;
        }
        private bool IsAdmin() => User.IsInRole("Admin");

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<PostResponseDTO>>> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetAllAsync(search, GetUserId(), page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostResponseDTO>> GetById(int id)
        {
            var post = await _postService.GetByIdAsync(id, GetUserId());
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết" });
            }
            return Ok(post);
        }

        [HttpGet("my-posts")]
        public async Task<ActionResult<PagedResult<PostResponseDTO>>> GetMyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetByAuthorAsync(GetUserId(), GetUserId(), page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PostResponseDTO>> Create([FromForm] CreatePostDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var imageUrls = new List<string>();

            // Handle multiple file uploads
            if (dto.ImageFiles != null && dto.ImageFiles.Any())
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posts");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        imageUrls.Add($"/images/posts/{fileName}");
                    }
                }
            }
            // Fallback to single ImageFile for backward compatibility
            else if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posts");
                
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }
                
                imageUrls.Add($"/images/posts/{fileName}");
            }

            var post = await _postService.CreateAsync(dto, GetUserId(), imageUrls);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PostResponseDTO>> Update(int id, [FromBody] CreatePostDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _postService.UpdateAsync(id, dto, GetUserId());
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết hoặc bạn không có quyền chỉnh sửa" });
            }

            return Ok(post);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _postService.DeleteAsync(id, GetUserId(), IsAdmin());
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy bài viết hoặc bạn không có quyền xóa" });
            }

            return Ok(new { message = "Đã xóa bài viết" });
        }
    }
}
