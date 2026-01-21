using System;
using System.Linq;
using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.Services.Shipping
{
    // Factory Pattern: Create shipping provider instances
    public interface IShippingProviderFactory
    {
        IShippingProvider GetProvider(string providerName);
        IEnumerable<string> GetAvailableProviders();
    }

    public class ShippingProviderFactory : IShippingProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _providers;

        public ShippingProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            
            // Register available providers
            _providers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "GHN", typeof(GHNShippingProvider) },
                { "Mock", typeof(MockShippingProvider) }
            };
        }

        public IShippingProvider GetProvider(string providerName)
        {
            if (!_providers.ContainsKey(providerName))
            {
                throw new ArgumentException($"Unknown shipping provider: {providerName}");
            }

            var providerType = _providers[providerName];
            return (IShippingProvider)_serviceProvider.GetService(providerType)!;
        }

        public IEnumerable<string> GetAvailableProviders()
        {
            return _providers.Keys;
        }
    }
}
