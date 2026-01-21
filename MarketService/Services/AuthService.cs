using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketService.Models;
using MarketService.DTOs;
using MarketService.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketService.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
        Task<AuthResponseDTO> RequestPasswordResetAsync(ForgotPasswordDTO dto);
        Task<AuthResponseDTO> ResetPasswordAsync(ResetPasswordDTO dto);
        string GenerateJwtToken(User user);
        int? ValidateToken(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly MarketDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(MarketDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            // Check if username exists
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Tên đăng nhập đã tồn tại"
                };
            }

            // Check if email exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Email đã được sử dụng"
                };
            }

            // Hash password with BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Đăng ký thành công",
                Token = token,
                User = MapToUserResponse(user)
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

            if (user == null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Tài khoản đã bị khóa"
                };
            }

            // Verify password with BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                };
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                User = MapToUserResponse(user)
            };
        }

        public async Task<AuthResponseDTO> RequestPasswordResetAsync(ForgotPasswordDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                // Return success even if email not found to prevent enumeration
                return new AuthResponseDTO { Success = true, Message = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu sẽ được gửi." };
            }

            // Generate Token
            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

            await _context.SaveChangesAsync();

            // TODO: Send Email
            // For now, we will return the token in the message for testing purposes
            Console.WriteLine($"[DEBUG] Reset Token for {user.Email}: {token}");

            return new AuthResponseDTO 
            { 
                Success = true, 
                Message = $"Vui lòng kiểm tra email để đặt lại mật khẩu. (DEBUG Token: {token})" 
            };
        }

        public async Task<AuthResponseDTO> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);

            if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return new AuthResponseDTO { Success = false, Message = "Token không hợp lệ hoặc đã hết hạn" };
            }

            // Update Password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return new AuthResponseDTO { Success = true, Message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập." };
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? "YourSecretKeyHere12345678901234567890"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "MarketService",
                audience: _config["Jwt:Audience"] ?? "MarketService",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? "YourSecretKeyHere12345678901234567890");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"] ?? "MarketService",
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"] ?? "MarketService",
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

                return userId;
            }
            catch
            {
                return null;
            }
        }

        private UserResponseDTO MapToUserResponse(User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
