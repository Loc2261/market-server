using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services.Shipping
{
    // Strategy Pattern: Interface for different shipping providers
    public interface IShippingProvider
    {
        string ProviderName { get; }
        
        // Calculate shipping fee
        Task<decimal> CalculateFeeAsync(
            string fromProvince, 
            string fromDistrict,
            string toProvince, 
            string toDistrict,
            decimal weight);
        
        // Create shipping order
        Task<string> CreateOrderAsync(
            ShippingOrder order,
            ShippingAddress pickupAddress,
            ShippingAddress deliveryAddress);
        
        // Track order
        Task<ShippingTrackingInfo> TrackOrderAsync(string trackingNumber);
    }

    // DTO for tracking info
    public class ShippingTrackingInfo
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }
        public List<TrackingEvent> Events { get; set; } = new();
    }

    public class TrackingEvent
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
