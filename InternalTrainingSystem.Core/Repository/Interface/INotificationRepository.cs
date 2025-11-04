using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface INotificationRepository
    {
        Task SaveNotificationAsync(Notification courseNotification);
        Notification? GetNotificationByCourseAndType(int courseId, NotificationType type);
        Notification? GetNotificationByClassAndType(int classId, NotificationType type);
        Task<bool> HasRecentNotification(NotificationType type, int courseId, int? days = 7);
        Task DeleteOldNotificationsAsync(int courseId, NotificationType type);
        Task<List<Notification>> GetNotificationsAsync(string? userId = null, string? roleName = null);
    }
}
