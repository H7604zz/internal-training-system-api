using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public NotificationType Type { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SentAt { get; set; }

        public int? CourseId { get; set; }
        public string? UserId { get; set; }
        public int? ClassId { get; set; }

        public bool IsRead { get; set; } = false;

        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
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

        // --- Notification types for User ---  
        UserWelcome = 6,
        UserWarning = 7,
        UserInfoUpdate = 8,

        // --- Notification types for System ---  
        SystemAlert = 9,
        SystemMaintenance = 10,

        // --- Notification types for Class ---  
        ClassCreated = 11,
        ClassUpdated = 12,
        ClassCancelled = 13,
        ClassReminder = 14,
        ClassAttendance = 15,

        // --- For custom or unspecified cases ---  
        Custom = 99

    }
}
