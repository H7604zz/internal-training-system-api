using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.Core.Constants;

namespace InternalTrainingSystem.Core.Models
{
    public class Attendance
    {
        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = AttendanceConstants.Status.NotYet; // Present, Absent, Late, LeftEarly

        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú về tình trạng điểm danh

        public bool IsExcused { get; set; } = false; // Có được phép vắng mặt không

        [StringLength(200)]
        public string? ExcuseReason { get; set; } // Lý do xin phép vắng mặt

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ScheduleId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; } = null!;
    }
}
