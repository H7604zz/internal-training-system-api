using InternalTrainingSystem.Core.Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class ClassSwap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RequesterId { get; set; } = string.Empty;

        [ForeignKey(nameof(RequesterId))]
        public virtual ApplicationUser Requester { get; set; } = null!;

        [Required]
        public string TargetId { get; set; } = string.Empty;

        [ForeignKey(nameof(TargetId))]
        public virtual ApplicationUser Target { get; set; } = null!;

        [Required]
        public int FromClassId { get; set; }

        [ForeignKey(nameof(FromClassId))]
        public virtual Class FromClass { get; set; } = null!;

        [Required]
        public int ToClassId { get; set; }

        [ForeignKey(nameof(ToClassId))]
        public virtual Class ToClass { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = ClassSwapConstants.Pending; // Pending, Approved, Rejected, Cancelled

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public string? RespondedById { get; set; }

        [ForeignKey(nameof(RespondedById))]
        public virtual ApplicationUser? RespondedBy { get; set; }
    }
}
