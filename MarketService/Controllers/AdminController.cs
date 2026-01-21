using Microsoft.AspNetCore.Mvc;
using MarketService.Services;
using Microsoft.AspNetCore.Authorization;
using MarketService.DTOs;

namespace MarketService.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IProductService _productService;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        public AdminController(IAdminService adminService, IProductService productService, IPostService postService, ICategoryService categoryService, IUserService userService)
        {
            _adminService = adminService;
            _productService = productService;
            _postService = postService;
            _categoryService = categoryService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("api/admin/stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("api/admin/products")]
        public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // For now use the standard GetAllAsync (Available only)
            // In a real app we'd have a specific AdminGetAllAsync
            var products = await _productService.GetAllAsync(search: search, page: page, pageSize: pageSize);
            return Ok(products);
        }

        [HttpDelete("api/admin/products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteAsync(id, 0, true);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("api/admin/products/cleanup-categories")]
        public async Task<IActionResult> CleanupProducts()
        {
            var count = await _productService.CleanupInvalidProductsAsync();
            return Ok(new { count });
        }

        [HttpGet("api/admin/posts")]
        public async Task<IActionResult> GetPosts([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetAllAsync(search, 0, page, pageSize);
            return Ok(result);
        }

        [HttpDelete("api/admin/posts/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var result = await _postService.DeleteAsync(id, 0, true);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet("api/admin/users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userService.GetAllAsync(search, page, pageSize);
            return Ok(users);
        }

        [HttpPost("api/admin/users/{id}/set-role")]
        public async Task<IActionResult> SetUserRole(int id, [FromBody] SetRoleDTO dto)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null && user.Username == "admin")
            {
                return BadRequest(new { message = "Không thể thay đổi vai trò của quản trị viên gốc." });
            }

            var result = await _userService.SetRoleAsync(id, dto.Role);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("api/admin/users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null && user.Username == "admin")
            {
                return BadRequest(new { message = "Không thể thay đổi trạng thái của quản trị viên gốc." });
            }

            var result = await _userService.ToggleActiveAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("api/admin/users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user != null && user.Username == "admin")
            {
                return BadRequest(new { message = "Không thể xóa quản trị viên gốc." });
            }

            var result = await _userService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        // Category Support
        [HttpGet("api/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAllAsync(true);
            return Ok(categories);
        }

        [HttpGet("api/admin/categories")]
        public async Task<IActionResult> AdminGetCategories([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var categories = await _categoryService.GetPagedAsync(search, page, pageSize);
            return Ok(categories);
        }

        [HttpPost("api/admin/categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var category = await _categoryService.CreateAsync(dto);
            return Ok(category);
        }

        [HttpPut("api/admin/categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var category = await _categoryService.UpdateAsync(id, dto);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpDelete("api/admin/categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("api/admin/categories/{id}/toggle-active")]
        public async Task<IActionResult> ToggleCategoryActive(int id)
        {
            var result = await _categoryService.ToggleActiveAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Posts()
        {
            return View();
        }

        public IActionResult Analytics()
        {
            return View();
        }

        public IActionResult Categories()
        {
            return View();
        }
    }
}
