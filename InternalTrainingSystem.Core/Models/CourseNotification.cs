using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class CourseNotification
    {
        [Key]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public NotificationType Type { get; set; } // enum: Start, Reminder, Finish, etc.
        public DateTime SentAt { get; set; }
    }

    public enum NotificationType
    {
        Start = 1,        
        Reminder = 2,        
        Finish = 3,        
        StaffConfirm = 4,  
        Certificate = 5
    }
}
