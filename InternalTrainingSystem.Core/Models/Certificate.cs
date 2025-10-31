using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class Certificate
    {
        [Key]
        public int CertificateId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int CourseId { get; set; }

        [Required]
        [MaxLength(255)]
        public string CertificateName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Now;

        public DateTime? ExpirationDate { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}
