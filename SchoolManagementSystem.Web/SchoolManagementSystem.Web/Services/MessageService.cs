using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public class MessageService : BaseService<MessageService>
    {
        private readonly SchoolDbContext _context;

        public MessageService(SchoolDbContext context, ILogger<MessageService> logger) : base(logger)
        {
            _context = context;
        }

        public async Task<List<Message>> GetMessagesForUserAsync(string userId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _context.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ReceiverId == userId)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();
            }, $"Error occurred while retrieving messages for user {userId}", new List<Message>());
        }

        public async Task<List<Message>> GetSentMessagesAsync(string userId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                 return await _context.Messages
                    .Include(m => m.Receiver)
                    .Where(m => m.SenderId == userId)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();
            }, $"Error occurred while retrieving sent messages for user {userId}", new List<Message>());
        }

        public async Task SendMessageAsync(string senderId, string receiverEmail, string subject, string content)
        {
            await ExecuteSafeAsync(async () =>
            {
                var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Email == receiverEmail);
                if (receiver == null) throw new Exception("Receiver not found");
    
                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiver.Id,
                    Subject = subject,
                    Content = content,
                    SentAt = DateTime.UtcNow
                };
    
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }, $"Error occurred while sending message from {senderId} to {receiverEmail}");
        }
        
        // Overload using ID directly if needed
        public async Task SendMessageByIdAsync(string senderId, string receiverId, string subject, string content)
        {
            await ExecuteSafeAsync(async () =>
            {
                 var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Subject = subject,
                    Content = content,
                    SentAt = DateTime.UtcNow
                };
    
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }, $"Error occurred while sending message from {senderId} to {receiverId}");
        }

        public async Task MarkMessageAsReadAsync(int messageId)
        {
            await ExecuteSafeAsync(async () =>
            {
                var msg = await _context.Messages.FindAsync(messageId);
                if (msg != null)
                {
                    msg.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while marking message {messageId} as read");
        }
    }
}
