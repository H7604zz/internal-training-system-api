using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface INotificationService
    {
        Task SaveNotificationAsync(Notification notification, List<string>? userIds = null, List<string>? roleNames = null);
        Notification? GetNotificationByCourseAndType(int courseId, NotificationType type);
        Notification? GetNotificationByClassAndType(int classId, NotificationType type);
        public bool HasRecentNotification(NotificationType type, int courseId, int days = 7);
        Task DeleteOldNotificationsAsync(int courseId, NotificationType type);
        Task<List<Notification>> GetNotificationsAsync(string? userId = null, string? roleName = null);
    }
}
