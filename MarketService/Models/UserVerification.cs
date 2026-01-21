using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketService.Models
{
    public enum VerificationType
    {
        Phone,
        Email,
        ID,
        Business,
        Face
    }

    public enum VerificationStatus
    {
        Pending,
        Verified,
        Rejected
    }

    public class UserVerification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public VerificationType Type { get; set; }

        [Required]
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        [StringLength(500)]
        public string? DocumentUrl { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
