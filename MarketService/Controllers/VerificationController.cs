using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketService.Services;
using MarketService.DTOs;

namespace MarketService.Controllers
{
    [Authorize]
    public class VerificationController : Controller
    {
        private readonly IVerificationService _verificationService;

        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var verifications = await _verificationService.GetUserVerificationsAsync(userId);
            var level = await _verificationService.GetVerificationLevelAsync(userId);

            ViewBag.CurrentLevel = level;
            return View(verifications);
        }

        [HttpPost]
        public async Task<IActionResult> RequestVerification(RequestVerificationDTO dto)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _verificationService.RequestVerificationAsync(userId, dto);
            
            return RedirectToAction("Index");
        }
    }
}
