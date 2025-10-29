using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface INotificationService
    {
        Task SaveNotificationAsync(Notification courseNotification);
        Notification? GetNotificationByCourseAndType(int courseId, NotificationType type);
        Notification? GetNotificationByUserAndType(string userId, NotificationType type);
        Notification? GetNotificationByClassAndType(int classId, NotificationType type);
        public bool HasRecentNotification(NotificationType type, int courseId, int days = 7);
        Task DeleteOldNotificationsAsync(int courseId, NotificationType type);
    }
}
