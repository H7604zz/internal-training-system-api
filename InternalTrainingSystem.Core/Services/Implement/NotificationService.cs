using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class NotificationService : INotificationService
    {

        private readonly INotificationRepository _notificationRepo;

        public NotificationService(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task SaveNotificationAsync(Notification notification, List<string>? userIds = null, List<string>? roleNames = null)
        {
            await _notificationRepo.SaveNotificationAsync(notification);
        }

        public Notification? GetNotificationByCourseAndType(int courseId, NotificationType type)
        {
            return _notificationRepo.GetNotificationByCourseAndType(courseId, type);
        }

        public Notification? GetNotificationByClassAndType(int classId, NotificationType type)
        {
            return _notificationRepo.GetNotificationByClassAndType(classId, type);
        }

        public bool HasRecentNotification(NotificationType type, int courseId, int days = 7)
        {
            return _notificationRepo.HasRecentNotification(type, courseId, days);
        }

        public async Task DeleteOldNotificationsAsync(int courseId, NotificationType type)
        {
           await _notificationRepo.DeleteOldNotificationsAsync(courseId, type);
        }

        public async Task<List<Notification>> GetNotificationsAsync(string? userId = null, string? roleName = null)
        {
           return await _notificationRepo.GetNotificationsAsync(userId, roleName);
        }

    }
}
