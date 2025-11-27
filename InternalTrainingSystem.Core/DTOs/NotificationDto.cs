using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.DTOs
{
    public class NotificationResponse
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public DateTime? SentAt { get; set; }
        public List<NotificationRecipientResponse> Recipients { get; set; }
    }

    public class NotificationRecipientResponse
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}

   