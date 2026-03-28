using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;
using MarketService.Helpers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace MarketService.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllAsync(bool onlyActive = true);
        Task<CategoryDTO?> GetByIdAsync(int id);
        Task<Category?> GetEntityByIdAsync(int id);
        Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto);
        Task<CategoryDTO?> UpdateAsync(int id, CreateCategoryDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<PagedResult<CategoryDTO>> GetPagedAsync(string? search = null, int page = 1, int pageSize = 10);
    }

    public class CategoryService : ICategoryService
    {
        private readonly MarketDbContext _context;

        public CategoryService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDTO>> GetAllAsync(bool onlyActive = true)
        {
            var query = _context.Categories.AsQueryable();
            if (onlyActive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count(p => !p.IsDeleted)
                })
                .ToListAsync();
        }

        public async Task<PagedResult<CategoryDTO>> GetPagedAsync(string? search = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search) || (c.Description != null && c.Description.Contains(search)));
            }

            var totalItems = await query.CountAsync();
            var items = await query.OrderBy(c => c.Name)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(c => new CategoryDTO
                                  {
                                      Id = c.Id,
                                      Name = c.Name,
                                      ImageUrl = c.ImageUrl,
                                      Description = c.Description,
                                      IsActive = c.IsActive,
                                      ProductCount = c.Products.Count(p => !p.IsDeleted)
                                  })
                                  .ToListAsync();

            return new PagedResult<CategoryDTO>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var c = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(cat => cat.Id == id);

            if (c == null) return null;

            return new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => !p.IsDeleted)
            };
        }

        public async Task<Category?> GetEntityByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.ImageFile != null)
            {
                category.ImageUrl = await SaveImageAsync(dto.ImageFile);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToDTO(category);
        }

        public async Task<CategoryDTO?> UpdateAsync(int id, CreateCategoryDTO dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            category.Name = dto.Name;
            category.Description = dto.Description;

            if (dto.ImageFile != null)
            {
                category.ImageUrl = await SaveImageAsync(dto.ImageFile);
            }

            await _context.SaveChangesAsync();
            return MapToDTO(category);
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");
            
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            
            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return $"/images/categories/{fileName}";
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            // If category has products, don't delete or move to 'Other'?
            // For now, let's allow deletion only if empty, or just delete and let products have null CategoryId
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        private CategoryDTO MapToDTO(Category c)
        {
            return new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products != null ? c.Products.Count(p => !p.IsDeleted) : 0
            };
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
