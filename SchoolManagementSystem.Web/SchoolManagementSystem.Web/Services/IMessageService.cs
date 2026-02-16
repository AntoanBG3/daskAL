using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public interface IMessageService
    {
        Task<List<Message>> GetMessagesForUserAsync(string userId);
        Task<List<Message>> GetSentMessagesAsync(string userId);
        Task SendMessageAsync(string senderId, string receiverEmail, string subject, string content);
        Task SendMessageByIdAsync(string senderId, string receiverId, string subject, string content);
        Task MarkMessageAsReadAsync(int messageId);
    }
}
