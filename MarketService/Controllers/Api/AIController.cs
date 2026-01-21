using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketService.Services;

namespace MarketService.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }


        // POST api/ai/detect-category
        [Authorize]
        [HttpPost("detect-category")]
        public async Task<IActionResult> DetectCategory([FromBody] CategoryDetectionRequest request)
        {
            var suggestion = await _aiService.DetectCategoryAsync(request.Title, request.Description);
            return Ok(suggestion);
        }

        // POST api/ai/generate-title
        [Authorize]
        [HttpPost("generate-title")]
        public async Task<IActionResult> GenerateTitle([FromBody] TitleGenerationRequest request)
        {
            var title = await _aiService.GenerateTitleAsync(request.Category, request.Keywords);
            return Ok(new { title });
        }

        // POST api/ai/enhance-description
        [Authorize]
        [HttpPost("enhance-description")]
        public async Task<IActionResult> EnhanceDescription([FromBody] DescriptionEnhancementRequest request)
        {
            var enhanced = await _aiService.EnhanceDescriptionAsync(request.OriginalDescription, request.Category);
            return Ok(new { description = enhanced });
        }
    }

    // Request DTOs

    public class CategoryDetectionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class TitleGenerationRequest
    {
        public string Category { get; set; } = string.Empty;
        public string? Keywords { get; set; }
    }

    public class DescriptionEnhancementRequest
    {
        public string OriginalDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
