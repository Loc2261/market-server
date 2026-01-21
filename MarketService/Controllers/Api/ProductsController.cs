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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private bool IsAdmin() => User.IsInRole("Admin");

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<ProductResponseDTO>>> GetAll(
            [FromQuery] string? category, 
            [FromQuery] string? search,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? location,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetAllAsync(category, search, minPrice, maxPrice, location, sortBy, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponseDTO>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            }
            return Ok(product);
        }

        [HttpGet("my-products")]
        public async Task<ActionResult<PagedResult<ProductResponseDTO>>> GetMyProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetBySellerAsync(GetUserId(), page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponseDTO>> Create([FromForm] CreateProductDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Handle file uploads
            if (dto.ImageFiles != null && dto.ImageFiles.Any())
            {
                var uploadedUrls = new List<string>();
                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        uploadedUrls.Add(await SaveImageAsync(file));
                    }
                }
                dto.ImageUrls ??= new List<string>();
                dto.ImageUrls.AddRange(uploadedUrls);
            }
            else if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                dto.ImageUrl = await SaveImageAsync(dto.ImageFile);
            }

            var product = await _productService.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponseDTO>> Update(int id, [FromForm] CreateProductDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Handle file uploads
            if (dto.ImageFiles != null && dto.ImageFiles.Any())
            {
                var uploadedUrls = new List<string>();
                foreach (var file in dto.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        uploadedUrls.Add(await SaveImageAsync(file));
                    }
                }
                dto.ImageUrls ??= new List<string>();
                dto.ImageUrls.AddRange(uploadedUrls);
            }
            else if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                dto.ImageUrl = await SaveImageAsync(dto.ImageFile);
            }

            var product = await _productService.UpdateAsync(id, dto, GetUserId());
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm hoặc bạn không có quyền chỉnh sửa" });
            }

            return Ok(product);
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            
            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return $"/images/products/{fileName}";
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id, GetUserId(), IsAdmin());
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm hoặc bạn không có quyền xóa" });
            }

            return Ok(new { message = "Đã xóa sản phẩm" });
        }
    }
}
