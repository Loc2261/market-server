using Microsoft.Extensions.Configuration;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services.Shipping
{
    // Concrete implementation for GHN (Giao Hàng Nhanh)
    // TODO: Integrate with real GHN API
    public class GHNShippingProvider : IShippingProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GHNShippingProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Shipping:GHN:ApiKey"] ?? "";
            
            // Configure HttpClient
            _httpClient.BaseAddress = new Uri("https://online-gateway.ghn.vn/shiip/public-api/v2/");
            _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);
        }

        public string ProviderName => "GHN";

        public async Task<decimal> CalculateFeeAsync(
            string fromProvince, 
            string fromDistrict, 
            string toProvince, 
            string toDistrict, 
            decimal weight)
        {
            // TODO: Call GHN API to calculate fee
            // For now, use mock calculation
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                // Fallback to mock if no API key
                return await new MockShippingProvider().CalculateFeeAsync(
                    fromProvince, fromDistrict, toProvince, toDistrict, weight);
            }

            // Real implementation would call:
            // POST /shipping-order/fee
            // https://api.ghn.vn/home/docs/detail?id=65

            decimal baseFee = 20000;
            if (fromProvince != toProvince) baseFee += 25000;
            if (weight > 1) baseFee += (weight - 1) * 7000;

            return baseFee;
        }

        public async Task<string> CreateOrderAsync(
            ShippingOrder order, 
            ShippingAddress pickupAddress, 
            ShippingAddress deliveryAddress)
        {
            // TODO: Call GHN API to create order
            // POST /shipping-order/create
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                return await new MockShippingProvider().CreateOrderAsync(
                    order, pickupAddress, deliveryAddress);
            }

            // Generate tracking number
            return $"GHN{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        public async Task<ShippingTrackingInfo> TrackOrderAsync(string trackingNumber)
        {
            // TODO: Call GHN API to track
            // POST /shipping-order/detail
            
            return await new MockShippingProvider().TrackOrderAsync(trackingNumber);
        }
    }
}
