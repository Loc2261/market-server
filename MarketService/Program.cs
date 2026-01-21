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
                new Category { Name = "Điện tử", Slug = "dien-tu", Description = "Điện thoại, Laptop, Phụ kiện..." },
                new Category { Name = "Thời trang", Slug = "thoi-trang", Description = "Quần áo, Giày dép, Trang sức..." },
                new Category { Name = "Gia dụng", Slug = "gia-dung", Description = "Đồ dùng nhà bếp, Nội thất..." },
                new Category { Name = "Sách", Slug = "sach", Description = "Sách kỹ năng, Giáo trình, Truyện..." },
                new Category { Name = "Khác", Slug = "khac", Description = "Các loại mặt hàng khác" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();
            Console.WriteLine("Initial categories seeded.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

app.Run();
