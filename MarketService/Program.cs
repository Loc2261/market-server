using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using MarketService.Data;
using MarketService.Services;
using MarketService.Services.Shipping;
using MarketService.Hubs;
using MarketService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<MarketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// New enhanced services
builder.Services.AddScoped<IVerificationService, VerificationService>();
builder.Services.AddScoped<ISellerScoreService, SellerScoreService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Order Management services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();

// Shipping services with Strategy Pattern
builder.Services.AddHttpClient<GHNShippingProvider>();
builder.Services.AddScoped<MockShippingProvider>();
builder.Services.AddScoped<GHNShippingProvider>();
builder.Services.AddScoped<IShippingProviderFactory, ShippingProviderFactory>();
builder.Services.AddScoped<IShippingService, ShippingService>();


// Add Memory Cache for performance
builder.Services.AddMemoryCache();

// Add JWT Authentication
// Add JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "YourSecretKeyHere12345678901234567890";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MarketService",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MarketService",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 1. Check Authorization Header
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                return Task.CompletedTask;
            }

            // 2. Check Cookie
            if (context.Request.Cookies.ContainsKey("auth_token"))
            {
                context.Token = context.Request.Cookies["auth_token"];
            }
            // 3. Check Query String (SignalR)
            else if (context.Request.Query.ContainsKey("access_token"))
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Skip API requests - let them return 401
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                return Task.CompletedTask;
            }

            // For Views, redirect to Login
            context.Response.Redirect("/Account/Login");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

// Add MVC and API Controllers
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add SignalR
builder.Services.AddSignalR();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hub
app.MapHub<ChatHub>("/chathub");

// Map Controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seeding Admin Account
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MarketDbContext>();
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@market.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "System Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(adminUser);
            context.SaveChanges();
            Console.WriteLine("Admin account created: admin / admin123");
        }

        // Seeding Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Điện tử", Description = "Điện thoại, Laptop, Phụ kiện..." },
                new Category { Name = "Thời trang", Description = "Quần áo, Giày dép, Trang sức..." },
                new Category { Name = "Gia dụng", Description = "Đồ dùng nhà bếp, Nội thất..." },
                new Category { Name = "Sách", Description = "Sách kỹ năng, Giáo trình, Truyện..." },
                new Category { Name = "Khác", Description = "Các loại mặt hàng khác" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();
            Console.WriteLine("Initial categories seeded.");
        }

        // Seeding Sample Products for testing
        if (!context.Products.Any())
        {
            // Create a test seller user
            var seller = context.Users.FirstOrDefault(u => u.Role == "Admin");
            if (seller == null)
            {
                seller = new User
                {
                    Username = "seller_demo",
                    Email = "seller@market.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("seller123"),
                    FullName = "Curated Shop",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(seller);
                context.SaveChanges();
            }

            var catDienTu = context.Categories.FirstOrDefault(c => c.Name == "Điện tử");
            var catThoiTrang = context.Categories.FirstOrDefault(c => c.Name == "Thời trang");
            var catGiaDung = context.Categories.FirstOrDefault(c => c.Name == "Gia dụng");
            var catSach = context.Categories.FirstOrDefault(c => c.Name == "Sách");
            var catKhac = context.Categories.FirstOrDefault(c => c.Name == "Khác");

            var sampleProducts = new List<Product>
            {
                // Điện tử
                new Product { Title = "MacBook Pro 14\" M3 Pro", Description = "MacBook Pro 14 inch chip M3 Pro, 18GB RAM, 512GB SSD. Máy mới 100%, nguyên seal. Bảo hành Apple 12 tháng.", Price = 45900000, Category = "Điện tử", CategoryId = catDienTu?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Product { Title = "Sony WH-1000XM5 Headphones", Description = "Tai nghe chống ồn cao cấp Sony WH-1000XM5, màu đen. Âm thanh Hi-Res, chống ồn hàng đầu.", Price = 6990000, Category = "Điện tử", CategoryId = catDienTu?.Id, SellerId = seller.Id, Location = "TP. Hồ Chí Minh", ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Product { Title = "iPad Air M2 11 inch", Description = "iPad Air M2 11 inch 128GB WiFi. Màn hình Liquid Retina, chip M2, hỗ trợ Apple Pencil Pro.", Price = 16490000, Category = "Điện tử", CategoryId = catDienTu?.Id, SellerId = seller.Id, Location = "Đà Nẵng", ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Product { Title = "Camera Canon EOS R6 Mark II", Description = "Máy ảnh mirrorless full-frame Canon EOS R6 Mark II body only. 24.2MP, quay 4K 60fps.", Price = 52000000, Category = "Điện tử", CategoryId = catDienTu?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=600&q=80", CreatedAt = DateTime.UtcNow.AddHours(-5) },

                // Thời trang
                new Product { Title = "Áo Khoác Wool Oversize", Description = "Áo khoác dạ wool oversize, phong cách minimalist. Chất liệu wool blend cao cấp, lót lụa. Size M/L.", Price = 1890000, Category = "Thời trang", CategoryId = catThoiTrang?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1591047139829-d91aecb6caea?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Product { Title = "Giày Sneaker Leather White", Description = "Giày sneaker da trắng minimalist. Đế cao su, da bò thật nguyên tấm. Size 38-44.", Price = 2490000, Category = "Thời trang", CategoryId = catThoiTrang?.Id, SellerId = seller.Id, Location = "TP. Hồ Chí Minh", ImageUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new Product { Title = "Túi Tote Canvas Premium", Description = "Túi tote canvas dày dặn, phong cách tối giản Nhật Bản. Kích thước 40x35cm, quai da.", Price = 650000, Category = "Thời trang", CategoryId = catThoiTrang?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Product { Title = "Đồng Hồ Minimalist Steel", Description = "Đồng hồ mặt tròn dây thép không gỉ, thiết kế tối giản. Chống nước 5ATM, kính sapphire.", Price = 3200000, Category = "Thời trang", CategoryId = catThoiTrang?.Id, SellerId = seller.Id, Location = "TP. Hồ Chí Minh", ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=600&q=80", CreatedAt = DateTime.UtcNow.AddHours(-8) },

                // Gia dụng
                new Product { Title = "Đèn Bàn Atollo Lamp", Description = "Đèn bàn Atollo replica cao cấp. Chất liệu nhôm sơn tĩnh điện, ánh sáng ấm 3000K.", Price = 4500000, Category = "Gia dụng", CategoryId = catGiaDung?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057ab6fe?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Product { Title = "Ghế Lounge Chair Gỗ Sồi", Description = "Ghế lounge gỗ sồi nguyên khối, đệm bọc vải linen. Phong cách Scandinavian.", Price = 12800000, Category = "Gia dụng", CategoryId = catGiaDung?.Id, SellerId = seller.Id, Location = "TP. Hồ Chí Minh", ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Product { Title = "Bình Gốm Sứ Thủ Công", Description = "Bình gốm sứ thủ công Bát Tràng, men rạn truyền thống. Cao 25cm.", Price = 850000, Category = "Gia dụng", CategoryId = catGiaDung?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1578500494198-246f612d3b3d?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Product { Title = "Bàn Coffee Table Walnut", Description = "Bàn coffee gỗ walnut nguyên tấm, chân sắt sơn đen. Kích thước 100x50x45cm.", Price = 8500000, Category = "Gia dụng", CategoryId = catGiaDung?.Id, SellerId = seller.Id, Location = "Đà Nẵng", ImageUrl = "https://images.unsplash.com/photo-1532372320572-cda25653a26d?w=600&q=80", CreatedAt = DateTime.UtcNow.AddHours(-12) },

                // Sách
                new Product { Title = "Atomic Habits - James Clear", Description = "Sách Atomic Habits bản tiếng Việt. Tình trạng: mới 100%, bìa cứng.", Price = 185000, Category = "Sách", CategoryId = catSach?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Product { Title = "Sapiens - Yuval Noah Harari", Description = "Sapiens: Lược Sử Loài Người. Bản dịch tiếng Việt, bìa mềm.", Price = 220000, Category = "Sách", CategoryId = catSach?.Id, SellerId = seller.Id, Location = "TP. Hồ Chí Minh", ImageUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-6) },
                new Product { Title = "Bộ Sưu Tập Art Book", Description = "Bộ 3 cuốn art book về thiết kế nội thất, kiến trúc và typography. Nhập khẩu.", Price = 1450000, Category = "Sách", CategoryId = catSach?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=600&q=80", CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new Product { Title = "Nến Thơm Sáp Ong Artisan", Description = "Nến thơm sáp ong tự nhiên 100%, hương gỗ đàn hương. Thời gian cháy 40 giờ.", Price = 380000, Category = "Khác", CategoryId = catKhac?.Id, SellerId = seller.Id, Location = "Hà Nội", ImageUrl = "https://images.unsplash.com/photo-1602607616660-0e3db8f5b8ab?w=600&q=80", CreatedAt = DateTime.UtcNow.AddHours(-3) },
            };

            context.Products.AddRange(sampleProducts);
            context.SaveChanges();
            Console.WriteLine($"Seeded {sampleProducts.Count} sample products.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

app.Run();
