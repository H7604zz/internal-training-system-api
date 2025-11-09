using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public NotificationType Type { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime? SentAt { get; set; } = DateTime.Now;

        // Liên kết tới các thực thể khác (tuỳ loại thông báo)
        public int? CourseId { get; set; }
        public int? ClassId { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }

        // Danh sách người nhận (Navigation Property)
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    }

    public class NotificationRecipient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NotificationId { get; set; }

        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = default!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public string? RoleName { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }

    public enum NotificationType
    {
        // --- Notification types for Course ---
        Start = 1,
        Reminder = 2,
        Finish = 3,
        StaffConfirm = 4,
        Certificate = 5,
        CourseFinalized = 6,
        CourseApproved = 18,
        CourseRejected = 19,

        // --- Notification types for User ---
        UserWelcome = 7,
        UserWarning = 8,
        UserInfoUpdate = 9,
        UserSwapClass = 17,

        // --- Notification types for System ---
        SystemAlert = 10,
        SystemMaintenance = 11,

        // --- Notification types for Class ---
        ClassCreated = 12,
        ClassUpdated = 13,
        ClassCancelled = 14,
        ClassReminder = 15,
        ClassAttendance = 16,

        // --- For custom or unspecified cases ---
        Custom = 99
    }
}
