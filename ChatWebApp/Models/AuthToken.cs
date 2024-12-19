using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatWebApp.Models
{
    public class AuthToken
    {
        [Key]
        public int TokenId { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public string? TokenValue { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
