using System;
using System.ComponentModel.DataAnnotations;
using SchoolManagementSystem.Web.Models.Auth;

namespace SchoolManagementSystem.Web.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;
        public User? Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; } = string.Empty;
        public User? Receiver { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
