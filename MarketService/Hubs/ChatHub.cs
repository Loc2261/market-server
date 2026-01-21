using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using MarketService.Services;
using MarketService.DTOs;

namespace MarketService.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private static Dictionary<int, string> _userConnections = new Dictionary<int, string>();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                _userConnections[userId] = Context.ConnectionId;
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                _userConnections.Remove(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int receiverId, string content, string? imageUrl = null)
        {
            var senderId = GetUserId();
            if (senderId == 0) return;

            var dto = new SendMessageDTO
            {
                ReceiverId = receiverId,
                Content = content
            };

            var message = await _chatService.SendMessageAsync(senderId, dto, imageUrl);

            if (message != null)
            {
                // Send to sender
                await Clients.Caller.SendAsync("ReceiveMessage", message);

                // Send to receiver if online
                if (_userConnections.TryGetValue(receiverId, out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task MarkAsRead(int conversationId)
        {
            var userId = GetUserId();
            if (userId > 0)
            {
                var success = await _chatService.MarkAsReadAsync(conversationId, userId);
                if (success)
                {
                    // Thông báo cho những người khác trong cuộc trò chuyện (đặc biệt là người gửi)
                    // Ở đây đơn giản là gửi tới Group của cuộc trò chuyện
                    await Clients.Group($"conversation_{conversationId}").SendAsync("MessagesRead", new {
                        ConversationId = conversationId,
                        ReaderId = userId,
                        ReadAt = DateTime.UtcNow
                    });
                }
            }
        }

        public async Task SendTypingStatus(int conversationId, bool isTyping)
        {
            var senderId = GetUserId();
            if (senderId > 0)
            {
                // Gửi tới những người khác trong nhóm cuộc trò chuyện
                await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("TypingStatus", new {
                    ConversationId = conversationId,
                    UserId = senderId,
                    IsTyping = isTyping
                });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
