using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services
{
    public class AIService : IAIService
    {
        private readonly MarketDbContext _context;

        public AIService(MarketDbContext context)
        {
            _context = context;
        }


        public Task<CategorySuggestionDTO> DetectCategoryAsync(string title, string? description = null)
        {
            // Rule-based category detection
            // In production: use ML model or API

            var text = (title + " " + (description ?? "")).ToLower();

            // Define category keywords
            var categoryKeywords = new Dictionary<string, string[]>
            {
                { "Điện thoại", new[] { "iphone", "samsung", "phone", "điện thoại", "smartphone" } },
                { "Laptop", new[] { "laptop", "macbook", "máy tính", "dell", "asus", "lenovo" } },
                { "Thời trang", new[] { "áo", "quần", "váy", "giày", "dép", "túi", "thời trang" } },
                { "Đồ gia dụng", new[] { "tủ", "bàn", "ghế", "giường", "nồi", "chảo", "gia dụng" } },
                { "Xe cộ", new[] { "xe", "ô tô", "xe máy", "motor", "honda", "yamaha" } },
                { "Sách", new[] { "sách", "truyện", "tiểu thuyết", "giáo trình" } }
            };

            // Score each category
            var scores = new Dictionary<string, int>();
            foreach (var categoryKw in categoryKeywords)
            {
                var score = categoryKw.Value.Count(kw => text.Contains(kw));
                if (score > 0)
                {
                    scores[categoryKw.Key] = score;
                }
            }

            if (scores.Count == 0)
            {
                return Task.FromResult(new CategorySuggestionDTO
                {
                    Category = "Khác",
                    Confidence = 0.3
                });
            }

            // Get best match
            var bestCategory = scores.OrderByDescending(s => s.Value).First();
            var confidence = Math.Min(bestCategory.Value * 0.25, 0.95); // Cap at 95%

            return Task.FromResult(new CategorySuggestionDTO
            {
                Category = bestCategory.Key,
                Confidence = confidence
            });
        }

        public Task<string> GenerateTitleAsync(string category, string? keywords = null)
        {
            // Simple title generation
            // In production: use GPT/Gemini

            var templates = new Dictionary<string, string[]>
            {
                { "Điện thoại", new[] { "Điện thoại {keywords} mới 99%", "{keywords} chính hãng giá tốt", "Bán {keywords} đẹp long lanh" } },
                { "Laptop", new[] { "Laptop {keywords} nguyên zin", "{keywords} chạy mượt like new", "Bán {keywords} giá sinh viên" } },
                { "Thời trang", new[] { "{keywords} đẹp mới 100%", "Áo {keywords} hàng chuẩn", "{keywords} xinh xắn giá rẻ" } }
            };

            if (templates.ContainsKey(category) && !string.IsNullOrEmpty(keywords))
            {
                var template = templates[category][new Random().Next(templates[category].Length)];
                return Task.FromResult(template.Replace("{keywords}", keywords));
            }

            return Task.FromResult(keywords ?? "Sản phẩm mới");
        }

        public Task<string> EnhanceDescriptionAsync(string originalDescription, string category)
        {
            // Simple description enhancement
            // In production: use AI to add selling points

            var enhancements = new Dictionary<string, string>
            {
                { "Điện thoại", "\n\n✅ Máy nguyên zin, không trầy xước\n✅ Pin trâu, sử dụng tốt\n✅ Hỗ trợ trả góp 0%" },
                { "Laptop", "\n\n✅ Máy chạy mượt, không lag\n✅ Bảo hành 6 tháng\n✅ Tặng kèm chuột + túi xách" },
                { "Thời trang", "\n\n✅ Hàng mới 100%\n✅ Chất liệu cao cấp\n✅ Freeship nội thành" }
            };

            var enhancement = enhancements.ContainsKey(category) ? enhancements[category] : "\n\n✅ Sản phẩm chất lượng\n✅ Giá cả hợp lý";
            
            return Task.FromResult(originalDescription + enhancement);
        }

        // Helper methods

    }
}
