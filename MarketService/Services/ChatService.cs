using Microsoft.EntityFrameworkCore;
using MarketService.Data;
using MarketService.Models;
using MarketService.DTOs;

namespace MarketService.Services
{
    public interface IChatService
    {
        Task<List<ConversationResponseDTO>> GetConversationsAsync(int userId);
        Task<List<MessageResponseDTO>> GetMessagesAsync(int conversationId, int userId);
        Task<MessageResponseDTO?> SendMessageAsync(int senderId, SendMessageDTO dto, string? imageUrl = null);
        Task<bool> MarkAsReadAsync(int conversationId, int userId);
        Task<Conversation?> GetOrCreateConversationAsync(int user1Id, int user2Id);
        Task<int> GetTotalUnreadCountAsync(int userId);
    }

    public class ChatService : IChatService
    {
        private readonly MarketDbContext _context;

        public ChatService(MarketDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConversationResponseDTO>> GetConversationsAsync(int userId)
        {
            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

            var result = new List<ConversationResponseDTO>();

            foreach (var c in conversations)
            {
                var otherUser = c.User1Id == userId ? c.User2 : c.User1;
                var lastMessage = c.Messages.FirstOrDefault();
                var unreadCount = await _context.Messages
                    .CountAsync(m => m.ConversationId == c.Id && m.SenderId != userId && !m.IsRead);

                result.Add(new ConversationResponseDTO
                {
                    Id = c.Id,
                    OtherUserId = otherUser.Id,
                    OtherUserName = otherUser.FullName ?? otherUser.Username,
                    OtherUserUsername = otherUser.Username,
                    OtherUserAvatar = otherUser.AvatarUrl,
                    LastMessage = lastMessage?.Content,
                    LastMessageAt = lastMessage != null ? DateTime.SpecifyKind(lastMessage.SentAt, DateTimeKind.Utc) : null,
                    UnreadCount = unreadCount
                });
            }

            return result;
        }

        public async Task<List<MessageResponseDTO>> GetMessagesAsync(int conversationId, int userId)
        {
            // Verify user is part of conversation
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId &&
                    (c.User1Id == userId || c.User2Id == userId));

            if (conversation == null)
            {
                return new List<MessageResponseDTO>();
            }

            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageResponseDTO
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName ?? m.Sender.Username,
                    SenderUsername = m.Sender.Username,
                    SenderAvatar = m.Sender.AvatarUrl,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    ImageUrl = m.ImageUrl,
                    SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc),
                    ReadAt = m.ReadAt.HasValue ? DateTime.SpecifyKind(m.ReadAt.Value, DateTimeKind.Utc) : null
                })
                .ToListAsync();
        }

        public async Task<MessageResponseDTO?> SendMessageAsync(int senderId, SendMessageDTO dto, string? imageUrl = null)
        {
            var conversation = await GetOrCreateConversationAsync(senderId, dto.ReceiverId);
            if (conversation == null)
            {
                return null;
            }

            var message = new Message
            {
                ConversationId = conversation.Id,
                SenderId = senderId,
                Content = dto.Content,
                ImageUrl = imageUrl,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            conversation.LastMessageAt = message.SentAt;

            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(senderId);

            return new MessageResponseDTO
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = sender?.FullName ?? sender?.Username ?? "Unknown",
                SenderUsername = sender?.Username ?? string.Empty,
                SenderAvatar = sender?.AvatarUrl,
                Content = message.Content,
                IsRead = message.IsRead,
                SentAt = DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc),
                ReadAt = message.ReadAt.HasValue ? DateTime.SpecifyKind(message.ReadAt.Value, DateTimeKind.Utc) : null,
                ImageUrl = message.ImageUrl
            };
        }

        public async Task<bool> MarkAsReadAsync(int conversationId, int userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .ToListAsync();

            foreach (var m in messages)
            {
                m.IsRead = true;
                m.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Conversation?> GetOrCreateConversationAsync(int user1Id, int user2Id)
        {
            if (user1Id == user2Id)
            {
                return null; // Can't chat with yourself
            }

            // Ensure consistent ordering
            var minId = Math.Min(user1Id, user2Id);
            var maxId = Math.Max(user1Id, user2Id);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == minId && c.User2Id == maxId) ||
                    (c.User1Id == maxId && c.User2Id == minId));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = minId,
                    User2Id = maxId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return conversation;
        }

        public async Task<int> GetTotalUnreadCountAsync(int userId)
        {
            return await _context.Messages
                .CountAsync(m => m.SenderId != userId && !m.IsRead && 
                    (m.Conversation.User1Id == userId || m.Conversation.User2Id == userId));
        }
    }
}
