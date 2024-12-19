using System.ComponentModel.DataAnnotations.Schema;

namespace ChatWebApp.Models
{
    public class Chat
    {
        public int ChatId { get; set; }
        [ForeignKey("AdminUserId")]
        public User? AdminUser { get; set; }
        public int? AdminUserId { get; set; }
        [ForeignKey("ParticipantUserId")]
        public User? ParticipantUser { get; set; }
        public int? ParticipantUserId { get; set; }
        
        // added here 
        
        public virtual List<Message> Messages { get; set; } = new List<Message>();
    }
}
