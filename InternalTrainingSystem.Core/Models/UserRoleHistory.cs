using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class UserRoleHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Added, Removed, Modified

        public DateTime ActionDate { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? ActionBy { get; set; } // UserId của người thực hiện thay đổi

        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú lý do thay đổi

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("ActionBy")]
        public virtual ApplicationUser? ActionByUser { get; set; }
    }
}