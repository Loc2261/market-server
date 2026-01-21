using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services.Shipping
{
    // Mock implementation for development/testing
    public class MockShippingProvider : IShippingProvider
    {
        public string ProviderName => "MockProvider";

        public async Task<decimal> CalculateFeeAsync(
            string fromProvince, 
            string fromDistrict, 
            string toProvince, 
            string toDistrict, 
            decimal weight)
        {
            // Mock calculation based on simple rules
            await Task.Delay(100); // Simulate API call

            decimal baseFee = 15000; // 15k VND base

            // Different province = +20k
            if (fromProvince != toProvince)
            {
                baseFee += 20000;
            }

            // Weight surcharge (per kg over 1kg)
            if (weight > 1)
            {
                baseFee += (weight - 1) * 5000;
            }

            return baseFee;
        }

        public async Task<string> CreateOrderAsync(
            ShippingOrder order, 
            ShippingAddress pickupAddress, 
            ShippingAddress deliveryAddress)
        {
            await Task.Delay(100); // Simulate API call
            
            // Generate mock tracking number
            var trackingNumber = $"MOCK{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            
            return trackingNumber;
        }

        public async Task<ShippingTrackingInfo> TrackOrderAsync(string trackingNumber)
        {
            await Task.Delay(100); // Simulate API call

            return new ShippingTrackingInfo
            {
                TrackingNumber = trackingNumber,
                Status = "InTransit",
                CurrentLocation = "Kho trung chuyển HCM",
                EstimatedDelivery = DateTime.Now.AddDays(2),
                Events = new List<TrackingEvent>
                {
                    new TrackingEvent
                    {
                        Timestamp = DateTime.Now.AddHours(-2),
                        Status = "PickedUp",
                        Location = "Kho lấy hàng",
                        Description = "Đã lấy hàng thành công"
                    },
                    new TrackingEvent
                    {
                        Timestamp = DateTime.Now.AddMinutes(-30),
                        Status = "InTransit",
                        Location = "Kho trung chuyển HCM",
                        Description = "Đang vận chuyển"
                    }
                }
            };
        }
    }
}
