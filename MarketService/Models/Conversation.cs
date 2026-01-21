using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public int User1Id { get; set; }

        public int User2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastMessageAt { get; set; }

        // Navigation properties
        [ForeignKey("User1Id")]
        public virtual User User1 { get; set; } = null!;

        [ForeignKey("User2Id")]
        public virtual User User2 { get; set; } = null!;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
