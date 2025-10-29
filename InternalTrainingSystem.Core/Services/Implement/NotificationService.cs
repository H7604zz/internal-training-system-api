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

        public async Task SaveNotificationAsync(Notification courseNotification)
        {
            await _notificationRepo.SaveNotificationAsync(courseNotification);
        }

        public Notification? GetNotificationByCourseAndType(int courseId, NotificationType type)
        {
            return _notificationRepo.GetNotificationByCourseAndType(courseId, type);
        }

        public Notification? GetNotificationByUserAndType(string userId, NotificationType type)
        {
            return _notificationRepo.GetNotificationByUserAndType(userId, type);
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
    }
}
