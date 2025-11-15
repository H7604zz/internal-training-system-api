using InternalTrainingSystem.Core.Configuration.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class ScheduleParticipant
    {
        [Key]
        public int ParticipantId { get; set; }

        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = ScheduleConstants.ParticipantStatus.InProgress;

        public DateTime? AttendanceDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Foreign Keys
        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}