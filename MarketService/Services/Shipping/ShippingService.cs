using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services.Shipping
{
    public interface IShippingService
    {
        // Address management
        Task<ShippingAddress> AddAddressAsync(int userId, CreateAddressDTO dto);
        Task<List<ShippingAddress>> GetUserAddressesAsync(int userId);
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
        Task<bool> DeleteAddressAsync(int userId, int addressId);
        
        // Shipping calculation
        Task<List<ShippingFeeDTO>> CalculateAllProvidersAsync(
            int productId, 
            int deliveryAddressId);
        
        Task<decimal> CalculateFeeAsync(
            int productId, 
            int deliveryAddressId, 
            string provider);
        
        // Order management
        Task<ShippingOrder> CreateShippingOrderAsync(CreateShippingOrderDTO dto);
        Task<ShippingTrackingInfo> TrackOrderAsync(string trackingNumber);
        Task<List<ShippingOrder>> GetUserOrdersAsync(int userId);
        Task<bool> DeleteOrderAsync(int orderId, int userId);
    }

    public class ShippingService : IShippingService
    {
        private readonly MarketDbContext _context;
        private readonly IShippingProviderFactory _providerFactory;

        public ShippingService(
            MarketDbContext context,
            IShippingProviderFactory providerFactory)
        {
            _context = context;
            _providerFactory = providerFactory;
        }

        public async Task<ShippingAddress> AddAddressAsync(int userId, CreateAddressDTO dto)
        {
            // If this is first address or marked as default, set as default
            var existingCount = await _context.ShippingAddresses
                .CountAsync(a => a.UserId == userId);

            var address = new ShippingAddress
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Province = dto.Province,
                District = dto.District,
                Ward = dto.Ward,
                AddressLine = dto.AddressLine,
                IsDefault = dto.IsDefault || existingCount == 0,
                CreatedAt = DateTime.UtcNow
            };

            // If setting as default, unset others
            if (address.IsDefault)
            {
                var others = await _context.ShippingAddresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .ToListAsync();
                
                foreach (var other in others)
                {
                    other.IsDefault = false;
                }
            }

            _context.ShippingAddresses.Add(address);
            await _context.SaveChangesAsync();

            return address;
        }

        public async Task<List<ShippingAddress>> GetUserAddressesAsync(int userId)
        {
            return await _context.ShippingAddresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            var address = await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            
            if (address == null) return false;

            // Unset all others
            var others = await _context.ShippingAddresses
                .Where(a => a.UserId == userId && a.Id != addressId && a.IsDefault)
                .ToListAsync();
            
            foreach (var other in others)
            {
                other.IsDefault = false;
            }

            address.IsDefault = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            var address = await _context.ShippingAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            
            if (address == null) return false;

            _context.ShippingAddresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ShippingFeeDTO>> CalculateAllProvidersAsync(
            int productId, 
            int deliveryAddressId)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null) throw new ArgumentException("Product not found");

            var deliveryAddress = await _context.ShippingAddresses.FindAsync(deliveryAddressId);
            if (deliveryAddress == null) throw new ArgumentException("Delivery address not found");

            // Get seller's default address (or assume same province)
            var pickupProvince = "TP. Hồ Chí Minh"; // TODO: Get from seller address
            var pickupDistrict = "Quận 1";

            var fees = new List<ShippingFeeDTO>();
            var providers = _providerFactory.GetAvailableProviders();

            foreach (var providerName in providers)
            {
                try
                {
                    var provider = _providerFactory.GetProvider(providerName);
                    var fee = await provider.CalculateFeeAsync(
                        pickupProvince,
                        pickupDistrict,
                        deliveryAddress.Province,
                        deliveryAddress.District,
                        1.0m // TODO: Get product weight
                    );

                    fees.Add(new ShippingFeeDTO
                    {
                        Provider = providerName,
                        Fee = fee,
                        EstimatedDelivery = DateTime.Now.AddDays(3) // TODO: Get from provider
                    });
                }
                catch
                {
                    // Skip provider if error
                }
            }

            return fees.OrderBy(f => f.Fee).ToList();
        }

        public async Task<decimal> CalculateFeeAsync(
            int productId, 
            int deliveryAddressId, 
            string providerName)
        {
            var fees = await CalculateAllProvidersAsync(productId, deliveryAddressId);
            var fee = fees.FirstOrDefault(f => f.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase));
            
            return fee?.Fee ?? 0;
        }

        public async Task<ShippingOrder> CreateShippingOrderAsync(CreateShippingOrderDTO dto)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            
            if (product == null) throw new ArgumentException("Product not found");

            var pickupAddress = await _context.ShippingAddresses.FindAsync(dto.PickupAddressId);
            if (pickupAddress == null)
            {
                // Fallback to seller's default address
                pickupAddress = await _context.ShippingAddresses
                    .FirstOrDefaultAsync(a => a.UserId == product.SellerId && a.IsDefault);
                
                // If still null, just take the first one they have
                if (pickupAddress == null)
                {
                    pickupAddress = await _context.ShippingAddresses
                        .FirstOrDefaultAsync(a => a.UserId == product.SellerId);
                }
            }

            var deliveryAddress = await _context.ShippingAddresses.FindAsync(dto.DeliveryAddressId);
            
            if (pickupAddress == null || deliveryAddress == null)
            {
                throw new ArgumentException("Địa chỉ lấy hàng hoặc giao hàng không hợp lệ.");
            }

            // Calculate fee if not provided
            decimal fee = dto.ShippingFee;
            if (fee <= 0)
            {
                 fee = await CalculateFeeAsync(dto.ProductId, dto.DeliveryAddressId, dto.Provider);
            }

            // Create order using provider
            var provider = _providerFactory.GetProvider(dto.Provider);
            
            var order = new ShippingOrder
            {
                ProductId = dto.ProductId,
                SellerId = product.SellerId,
                BuyerId = dto.BuyerId,
                Provider = dto.Provider,
                PickupAddress = FormatAddress(pickupAddress),
                DeliveryAddress = FormatAddress(deliveryAddress),
                ShippingFee = fee,
                Status = ShippingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.ShippingOrders.Add(order);
            await _context.SaveChangesAsync();

            // Create tracking number with provider
            var trackingNumber = await provider.CreateOrderAsync(order, pickupAddress, deliveryAddress);
            order.TrackingNumber = trackingNumber;
            order.EstimatedDelivery = DateTime.Now.AddDays(3);
            
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<ShippingTrackingInfo> TrackOrderAsync(string trackingNumber)
        {
            var order = await _context.ShippingOrders
                .FirstOrDefaultAsync(o => o.TrackingNumber == trackingNumber);
            
            if (order == null) throw new ArgumentException("Order not found");

            var provider = _providerFactory.GetProvider(order.Provider);
            return await provider.TrackOrderAsync(trackingNumber);
        }

        public async Task<List<ShippingOrder>> GetUserOrdersAsync(int userId)
        {
            return await _context.ShippingOrders
                .IgnoreQueryFilters()
                .Where(o => (o.BuyerId == userId && !o.IsDeletedByBuyer) || (o.SellerId == userId && !o.IsDeletedBySeller))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteOrderAsync(int orderId, int userId)
        {
            var order = await _context.ShippingOrders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Id == orderId);
            
            if (order == null) return false;
            
            // Check roles and set individual deletion flags
            if (order.BuyerId == userId)
            {
                order.IsDeletedByBuyer = true;
            }
            else if (order.SellerId == userId)
            {
                order.IsDeletedBySeller = true;
            }
            else
            {
                return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private string FormatAddress(ShippingAddress address)
        {
            return $"{address.AddressLine}, {address.Ward}, {address.District}, {address.Province}";
        }
    }
}
