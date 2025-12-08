using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public class MessageService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<MessageService> _logger;

        public MessageService(SchoolDbContext context, ILogger<MessageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Message>> GetMessagesForUserAsync(string userId)
        {
            try
            {
                return await _context.Messages
                    .Include(m => m.Sender)
                    .Where(m => m.ReceiverId == userId)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving messages for user {UserId}", userId);
                return new List<Message>();
            }
        }

        public async Task<List<Message>> GetSentMessagesAsync(string userId)
        {
            try
            {
                 return await _context.Messages
                    .Include(m => m.Receiver)
                    .Where(m => m.SenderId == userId)
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();
            }
             catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving sent messages for user {UserId}", userId);
                return new List<Message>();
            }
        }

        public async Task SendMessageAsync(string senderId, string receiverEmail, string subject, string content)
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending message from {SenderId} to {ReceiverEmail}", senderId, receiverEmail);
                throw;
            }
        }
        
        // Overload using ID directly if needed
        public async Task SendMessageByIdAsync(string senderId, string receiverId, string subject, string content)
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending message from {SenderId} to {ReceiverId}", senderId, receiverId);
                throw;
            }
        }

        public async Task MarkMessageAsReadAsync(int messageId)
        {
            try
            {
                var msg = await _context.Messages.FindAsync(messageId);
                if (msg != null)
                {
                    msg.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while marking message {MessageId} as read", messageId);
                throw;
            }
        }
    }
}
