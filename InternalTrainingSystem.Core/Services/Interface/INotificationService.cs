using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface INotificationService
    {
        Task SaveNotificationAsync(Notification notification, List<string>? userIds = null, List<string>? roleNames = null);
        Notification? GetNotificationByCourseAndType(int courseId, NotificationType type);
        Notification? GetNotificationByClassAndType(int classId, NotificationType type);
        Task<bool> HasRecentNotification(NotificationType type, int courseId, int? days = 7);
        Task DeleteOldNotificationsAsync(int courseId, NotificationType type);
        Task<List<NotificationResponse>> GetNotificationsAsync(string? userId = null, string? roleName = null);
        Task NotifyTrainingDepartmentAsync(int courseId);
    }
}
