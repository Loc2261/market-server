using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int AuthorId { get; set; }

        public bool IsPublished { get; set; } = true;

        public int SharesCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; } = null!;

        public virtual ICollection<PostImage> Images { get; set; } = new List<PostImage>();
        public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
        public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    }
}
