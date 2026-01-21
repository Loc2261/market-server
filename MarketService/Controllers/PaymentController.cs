using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketService.Services;

namespace MarketService.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        // GET: /Payment/VNPayReturn
        public async Task<IActionResult> VNPayReturn()
        {
            var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

            var isValid = await _paymentService.ValidateVNPayCallbackAsync(queryParams);

            if (isValid)
            {
                return RedirectToAction("PaymentSuccess");
            }
            else
            {
                return RedirectToAction("PaymentFailed");
            }
        }

        // GET: /Payment/PaymentSuccess
        public IActionResult PaymentSuccess()
        {
            return View();
        }

        // GET: /Payment/PaymentFailed
        public IActionResult PaymentFailed()
        {
            return View();
        }
    }
}
