using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly SmartSchoolDbContext _context;

        public ChatRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatConversation>> GetConversationsByUserIdAsync(int userId)
        {
            return await _context.ChatConversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ChatConversation> GetConversationWithMessagesAsync(int conversationId, int userId)
        {
            return await _context.ChatConversations
                .Include(c => c.Messages.OrderBy(m => m.SentAt)) // Ensure messages are ordered chronologically
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);
        }

        public async Task AddConversationAsync(ChatConversation conversation)
        {
            await _context.ChatConversations.AddAsync(conversation);
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
        }

        public void DeleteConversation(ChatConversation conversation)
        {
            _context.ChatConversations.Remove(conversation);
        }

        public async Task ClearConversationMessagesAsync(int conversationId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .ToListAsync();

            if (messages.Any())
            {
                _context.ChatMessages.RemoveRange(messages);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}