using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task SaveNotificationAsync(Notification courseNotification)
        {
            try
            {
                _context.Notifications.Add(courseNotification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{ex.Message}", ex);
            }
        }

        public Notification? GetNotificationByCourseAndType(int courseId, NotificationType type)
        {
            return _context.Notifications
                .FirstOrDefault(n => n.CourseId == courseId && n.Type == type);
        }

        public Notification? GetNotificationByUserAndType(string userId, NotificationType type)
        {
            return _context.Notifications
                .FirstOrDefault(n => n.UserId == userId && n.Type == type);
        }

        public Notification? GetNotificationByClassAndType(int classId, NotificationType type)
        {
            return _context.Notifications
                .FirstOrDefault(n => n.ClassId == classId && n.Type == type);
        }

        public bool HasRecentNotification(NotificationType type, int courseId, int days = 7)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            return _context.Notifications
                .Any(n =>
                    n.Type == type &&
                    n.CourseId == courseId &&
                    n.CreatedAt >= since
                );
        }

        public async Task DeleteOldNotificationsAsync(int courseId, NotificationType type)
        {
            var oldNotis = _context.Notifications
                .Where(n => n.CourseId == courseId && n.Type == type)
                .ToList();

            if (oldNotis.Any())
            {
                _context.Notifications.RemoveRange(oldNotis);
               await _context.SaveChangesAsync();
            }
        }
    }
}
