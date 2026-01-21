using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.Models;
using MarketService.DTOs;
using MarketService.Helpers;

namespace MarketService.Services
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponseDTO>> GetAllAsync(
            string? category = null, 
            string? search = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? location = null,
            string? sortBy = "newest",
            int page = 1,
            int pageSize = 10);
        Task<ProductResponseDTO?> GetByIdAsync(int id);
        Task<ProductResponseDTO> CreateAsync(CreateProductDTO dto, int sellerId);
        Task<ProductResponseDTO?> UpdateAsync(int id, CreateProductDTO dto, int userId);
        Task<bool> DeleteAsync(int id, int userId, bool isAdmin);
        Task<PagedResult<ProductResponseDTO>> GetBySellerAsync(int sellerId, int page = 1, int pageSize = 10);
        Task<int> CleanupInvalidProductsAsync();
    }

    public class ProductService : IProductService
    {
        private readonly MarketDbContext _context;

        public ProductService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductResponseDTO>> GetAllAsync(
            string? category = null, 
            string? search = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? location = null,
            string? sortBy = "newest",
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Products
                .Include(p => p.Seller)
                .Include(p => p.CategoryEntity)
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Available);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category || p.CategoryEntity.Name == category || p.CategoryEntity.Slug == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p => p.Location == location);
            }

            // Sorting
            query = sortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(p => p.CreatedAt),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await query.ToPagedResultAsync(page, pageSize, MapToResponse);
        }

        public async Task<ProductResponseDTO?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .Include(p => p.CategoryEntity)
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

            return product == null ? null : MapToResponse(product);
        }

        public async Task<ProductResponseDTO> CreateAsync(CreateProductDTO dto, int sellerId)
        {
            // Validate Category
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null || !category.IsActive)
            {
                throw new ArgumentException("Danh mục không hợp lệ hoặc đã bị ẩn.");
            }

            var imageUrls = dto.ImageUrls != null && dto.ImageUrls.Any() 
                ? string.Join(";", dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u))) 
                : dto.ImageUrl;

            var product = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Category = category.Name,
                ImageUrl = imageUrls,
                SellerId = sellerId,
                Status = ProductStatus.Available,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Load seller for response
            await _context.Entry(product).Reference(p => p.Seller).LoadAsync();

            return MapToResponse(product);
        }

        public async Task<ProductResponseDTO?> UpdateAsync(int id, CreateProductDTO dto, int userId)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || product.SellerId != userId)
            {
                return null;
            }

            // Validate Category
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null || !category.IsActive)
            {
                throw new ArgumentException("Danh mục không hợp lệ hoặc đã bị ẩn.");
            }

            var imageUrls = dto.ImageUrls != null && dto.ImageUrls.Any() 
                ? string.Join(";", dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u))) 
                : dto.ImageUrl;

            product.Title = dto.Title;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.Category = category.Name;
            product.Location = dto.Location ?? product.Location;
            product.ImageUrl = imageUrls ?? product.ImageUrl;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(product);
        }

        public async Task<bool> DeleteAsync(int id, int userId, bool isAdmin)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            if (product.SellerId != userId && !isAdmin)
            {
                return false;
            }

            // Soft delete
            product.IsDeleted = true;
            // _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<ProductResponseDTO>> GetBySellerAsync(int sellerId, int page = 1, int pageSize = 10)
        {
            var query = _context.Products
                .Include(p => p.Seller)
                .Include(p => p.CategoryEntity)
                .Where(p => !p.IsDeleted && p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt);

            return await query.ToPagedResultAsync(page, pageSize, MapToResponse);
        }

        public async Task<int> CleanupInvalidProductsAsync()
        {
            // Find products where CategoryId is null OR CategoryId does not exist in Categories table
            var invalidProducts = await _context.Products
                .Where(p => p.CategoryId == null || !_context.Categories.Any(c => c.Id == p.CategoryId))
                .ToListAsync();

            if (invalidProducts.Any())
            {
                _context.Products.RemoveRange(invalidProducts);
                await _context.SaveChangesAsync();
            }

            return invalidProducts.Count;
        }

        private static ProductResponseDTO MapToResponse(Product p)
        {
            var imageUrls = !string.IsNullOrEmpty(p.ImageUrl) 
                ? p.ImageUrl.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() 
                : new List<string>();

            return new ProductResponseDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = imageUrls.FirstOrDefault(),
                ImageUrls = imageUrls,
                Status = p.Status.ToString(),
                SellerId = p.SellerId,
                SellerName = p.Seller?.FullName ?? p.Seller?.Username ?? "Ẩn danh",
                SellerUsername = p.Seller?.Username ?? string.Empty,
                CategoryId = p.CategoryId,
                Category = p.CategoryEntity?.Name ?? p.Category ?? "Khác",
                Location = p.Location ?? "Toàn quốc",
                CreatedAt = p.CreatedAt
            };
        }
    }
}
