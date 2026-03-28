using Microsoft.AspNetCore.Mvc;
using MarketService.Services;
using System.Security.Claims;

namespace MarketService.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public HomeController(IProductService productService, IPostService postService, ICategoryService categoryService)
        {
            _productService = productService;
            _postService = postService;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int id)) userId = id;
            }

            var productsResult = await _productService.GetAllAsync(page: 1, pageSize: 8, currentUserId: userId);
            var postsResult = await _postService.GetAllAsync(page: 1, pageSize: 4);
            var categories = await _categoryService.GetAllAsync(onlyActive: true);

            ViewBag.Products = productsResult.Items;
            ViewBag.Posts = postsResult.Items;
            ViewBag.Categories = categories;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
