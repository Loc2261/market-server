using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MarketService.DTOs;
using MarketService.Services;

namespace MarketService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("conversations")]
        public async Task<ActionResult<List<ConversationResponseDTO>>> GetConversations()
        {
            var conversations = await _chatService.GetConversationsAsync(GetUserId());
            return Ok(conversations);
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<List<MessageResponseDTO>>> GetMessages(int conversationId)
        {
            var messages = await _chatService.GetMessagesAsync(conversationId, GetUserId());
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<ActionResult<MessageResponseDTO>> SendMessage([FromBody] SendMessageDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var message = await _chatService.SendMessageAsync(GetUserId(), dto);
            if (message == null)
            {
                return BadRequest(new { message = "Không thể gửi tin nhắn" });
            }

            return Ok(message);
        }

        [HttpPost("conversations/{conversationId}/read")]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            await _chatService.MarkAsReadAsync(conversationId, GetUserId());
            return Ok(new { message = "Đã đánh dấu đã đọc" });
        }

        [HttpPost("start/{userId}")]
        public async Task<ActionResult<ConversationResponseDTO>> StartConversation(int userId)
        {
            var conversation = await _chatService.GetOrCreateConversationAsync(GetUserId(), userId);
            if (conversation == null)
            {
                return BadRequest(new { message = "Không thể tạo cuộc hội thoại" });
            }

            return Ok(new { conversationId = conversation.Id });
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            return Ok(await _chatService.GetTotalUnreadCountAsync(GetUserId()));
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Tệp không hợp lệ");

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { imageUrl = $"/uploads/chat/{fileName}" });
        }
    }
}
