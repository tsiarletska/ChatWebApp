using ChatWebApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatWebApp.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int? ChatId { get; set; }
        [ForeignKey("ChatId")]
        public Chat? Chat { get; set; }
        public int? SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User? Sender { get; set; }
        public string? Text { get; set; }
        public DateTime? SentAt { get; set; }
        public string? Status { get; set; }

    }
}
