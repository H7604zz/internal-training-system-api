using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class NotificationService : INotificationService
    {

        private readonly ApplicationDbContext _context;

        public NotificationService( ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task SaveNotificationAsync(Notification notification,List<string>? userIds = null,List<string>? roleNames = null)
        {
            var recipients = new List<NotificationRecipient>();

            // Thêm người nhận theo userId cụ thể
            if (userIds != null && userIds.Any())
            {
                recipients.AddRange(userIds.Select(uid => new NotificationRecipient
                {
                    UserId = uid
                }));
            }

            // Thêm người nhận theo role cụ thể
            if (roleNames != null && roleNames.Any())
            {
                recipients.AddRange(roleNames.Select(r => new NotificationRecipient
                {
                    RoleName = r
                }));
            }
            notification.Recipients = recipients;

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
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
            var since = DateTime.Now.AddDays(-days);

            return _context.Notifications
                .Any(n =>
                    n.Type == type &&
                    n.CourseId == courseId &&
                    n.SentAt >= since
                );
        }

        public void DeleteOldNotifications(int courseId, NotificationType type)
        {
            var oldNotis = _context.Notifications
                .Where(n => n.CourseId == courseId && n.Type == type)
                .ToList();

            if (oldNotis.Any())
            {
                _context.Notifications.RemoveRange(oldNotis);
                _context.SaveChanges();
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
