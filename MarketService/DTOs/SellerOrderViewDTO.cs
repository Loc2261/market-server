using System;
using System.Collections.Generic;
using MarketService.Models;

namespace MarketService.DTOs
{
    public class SellerOrderViewDTO
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal FinalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public List<SellerOrderItemDTO> OrderItems { get; set; } = new();
    }

    public class SellerOrderItemDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
