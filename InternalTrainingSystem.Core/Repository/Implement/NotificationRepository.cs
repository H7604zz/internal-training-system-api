using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

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
                    n.SentAt >= since
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

        public async Task<List<Notification>> GetNotificationsAsync(string? userId = null, string? roleName = null)
        {
            var query = _context.Notifications
                .Include(n => n.Recipients)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(n => n.Recipients.Any(r => r.UserId == userId));
            }

            if (!string.IsNullOrEmpty(roleName))
            {
                query = query.Where(n => n.Recipients.Any(r => r.RoleName == roleName));
            }

            return await query
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }
    }
}
