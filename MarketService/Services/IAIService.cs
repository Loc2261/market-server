using MarketService.DTOs;

namespace MarketService.Services
{
    public interface IAIService
    {
        
        // Category detection
        Task<CategorySuggestionDTO> DetectCategoryAsync(string title, string? description = null);
        
        // Smart title generation
        Task<string> GenerateTitleAsync(string category, string? keywords = null);
        
        // Description enhancement
        Task<string> EnhanceDescriptionAsync(string originalDescription, string category);
    }
}
