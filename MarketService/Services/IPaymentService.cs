using MarketService.Models;

namespace MarketService.Services
{
    public interface IPaymentService
    {
        Task<Payment> ProcessCODPaymentAsync(int orderId);
        Task<string> CreateVNPayPaymentUrlAsync(int orderId, string returnUrl);
        Task<bool> ValidateVNPayCallbackAsync(Dictionary<string, string> queryParams);
        Task<Payment> RecordPaymentAsync(int orderId, PaymentMethod method, string? transactionId = null, string? responseData = null);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status);
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    }
}
