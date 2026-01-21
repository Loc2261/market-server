using MarketService.Data;
using MarketService.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;


namespace MarketService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly MarketDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentService(MarketDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Payment> ProcessCODPaymentAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Không tìm thấy đơn hàng");
            }

            var payment = new Payment
            {
                OrderId = orderId,
                Method = PaymentMethod.COD,
                Amount = order.FinalAmount,
                Status = PaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<string> CreateVNPayPaymentUrlAsync(int orderId, string returnUrl)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Không tìm thấy đơn hàng");
            }

            var vnpUrl = _configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var vnpTmnCode = _configuration["VNPay:TmnCode"] ?? "DEMOMERCHANT";
            var vnpHashSecret = _configuration["VNPay:HashSecret"] ?? "DEMOSECRETKEY";

            var vnpData = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnpTmnCode },
                { "vnp_Amount", ((long)(order.FinalAmount * 100)).ToString() }, // VNPay uses smallest unit
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", "127.0.0.1" },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan don hang #{orderId}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", orderId.ToString() }
            };

            // Build query string
            var queryString = string.Join("&", vnpData.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            // Create secure hash
            var signData = string.Join("&", vnpData.Select(kv => $"{kv.Key}={kv.Value}"));
            var vnpSecureHash = HmacSHA512(vnpHashSecret, signData);

            var paymentUrl = $"{vnpUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";

            return paymentUrl;
        }

        public async Task<bool> ValidateVNPayCallbackAsync(Dictionary<string, string> queryParams)
        {
            var vnpHashSecret = _configuration["VNPay:HashSecret"] ?? "DEMOSECRETKEY";

            if (!queryParams.ContainsKey("vnp_SecureHash"))
            {
                return false;
            }

            var vnpSecureHash = queryParams["vnp_SecureHash"];
            queryParams.Remove("vnp_SecureHash");
            queryParams.Remove("vnp_SecureHashType");

            var sortedParams = new SortedDictionary<string, string>(queryParams);
            var signData = string.Join("&", sortedParams.Select(kv => $"{kv.Key}={kv.Value}"));
            var checkSum = HmacSHA512(vnpHashSecret, signData);

            if (checkSum != vnpSecureHash)
            {
                return false;
            }

            // Check response code
            if (queryParams.ContainsKey("vnp_ResponseCode") && queryParams["vnp_ResponseCode"] == "00")
            {
                // Payment successful
                var orderId = int.Parse(queryParams["vnp_TxnRef"]);
                var transactionId = queryParams.ContainsKey("vnp_TransactionNo") ? queryParams["vnp_TransactionNo"] : null;

                await RecordPaymentAsync(orderId, PaymentMethod.VNPay, transactionId, signData);
                return true;
            }

            return false;
        }

        public async Task<Payment> RecordPaymentAsync(int orderId, PaymentMethod method, string? transactionId = null, string? responseData = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Không tìm thấy đơn hàng");
            }

            var payment = new Payment
            {
                OrderId = orderId,
                Method = method,
                Amount = order.FinalAmount,
                Status = PaymentStatus.Paid,
                TransactionId = transactionId,
                ResponseData = responseData,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Update order payment status
            order.PaymentStatus = PaymentStatus.Paid;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null) return false;

            payment.Status = status;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        private string HmacSHA512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
