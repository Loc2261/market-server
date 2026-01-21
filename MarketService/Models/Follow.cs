using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public class Follow
    {
        public int Id { get; set; }

        [Required]
        public int FollowerId { get; set; } // Người theo dõi

        [Required]
        public int FollowingId { get; set; } // Người được theo dõi

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual User Follower { get; set; } = null!;

        [ForeignKey("FollowingId")]
        public virtual User Following { get; set; } = null!;
    }
}
