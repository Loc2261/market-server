using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services;
using MarketService.DTOs;
using MarketService.ViewModels;
using MarketService.Data;
using MarketService.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketService.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly MarketDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(IProfileService profileService, MarketDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _profileService = profileService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("profile/{userId:int}")]
        [Route("profile/{username?}")]
        public async Task<IActionResult> Index(string? username, int? userId)
        {
            int? viewerId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                viewerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            int targetUserId;

            if (userId.HasValue)
            {
                // Search by ID
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return NotFound();
                targetUserId = user.Id;
            }
            else if (string.IsNullOrEmpty(username))
            {
                if (!viewerId.HasValue) return RedirectToAction("Login", "Account");
                targetUserId = viewerId.Value;
            }
            else
            {
                // Search by Username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return NotFound();
                targetUserId = user.Id;
            }

            var profile = await _profileService.GetPublicProfileAsync(targetUserId, viewerId);
            if (profile == null) return NotFound();

            // Get products and posts for display tabs
            var products = await _context.Products
                .Where(p => p.SellerId == targetUserId && p.Status != Models.ProductStatus.Hidden)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            var posts = await _context.Posts
                .Where(p => p.AuthorId == targetUserId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var orders = new List<ShippingOrder>();
            if (viewerId.HasValue && viewerId.Value == targetUserId)
            {
                orders = await _context.ShippingOrders
                    .IgnoreQueryFilters()
                    .Where(o => o.BuyerId == targetUserId || o.SellerId == targetUserId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }

            var model = new UserProfileViewModel
            {
                Profile = profile,
                IsOwner = viewerId.HasValue && viewerId.Value == targetUserId,
                Products = products,
                Posts = posts,
                Orders = orders
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        [Route("profile/edit")]
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = await _profileService.GetUserProfileAsync(userId);
            
            if (profile == null) return NotFound();

            return View(new UpdateProfileDTO
            {
                Username = profile.Username,
                Email = profile.Email,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Bio = profile.Bio,
                AvatarUrl = profile.AvatarUrl,
                CoverImageUrl = profile.CoverImageUrl
            });
        }

        [Authorize]
        [HttpPost]
        [Route("profile/edit")]
        public async Task<IActionResult> Edit(UpdateProfileDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Handle Avatar Upload
            if (dto.AvatarFile != null && dto.AvatarFile.Length > 0)
            {
                var fileName = $"avatar_{userId}_{Guid.NewGuid()}{Path.GetExtension(dto.AvatarFile.FileName)}";
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AvatarFile.CopyToAsync(stream);
                }
                dto.AvatarUrl = $"/uploads/profiles/{fileName}";
            }

            // Handle Cover Upload
            if (dto.CoverFile != null && dto.CoverFile.Length > 0)
            {
                var fileName = $"cover_{userId}_{Guid.NewGuid()}{Path.GetExtension(dto.CoverFile.FileName)}";
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.CoverFile.CopyToAsync(stream);
                }
                dto.CoverImageUrl = $"/uploads/profiles/{fileName}";
            }

            var success = await _profileService.UpdateProfileAsync(userId, dto);

            if (!success)
            {
                ModelState.AddModelError("", "Update failed");
                return View(dto);
            }

            return RedirectToAction("Index");
        }
    }
}
