using MarketService.DTOs;
using MarketService.Models;

namespace MarketService.ViewModels
{
    public class UserProfileViewModel
    {
        public UserProfileDTO Profile { get; set; }
        public bool IsOwner { get; set; }
        public List<Product> Products { get; set; } = new();
        public List<Post> Posts { get; set; } = new();
        public List<ShippingOrder> Orders { get; set; } = new();
    }
}
