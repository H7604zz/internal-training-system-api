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

        public async Task SaveNotificationAsync(
            Notification courseNotification,
            List<string>? userIds = null,
            List<string>? roleNames = null)
        {
            if (courseNotification == null)
                throw new ArgumentNullException(nameof(courseNotification));

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Notifications.Add(courseNotification);
                await _context.SaveChangesAsync();

                var recipients = new List<NotificationRecipient>();

                if (userIds != null && userIds.Any())
                {
                    recipients.AddRange(userIds.Select(userId => new NotificationRecipient
                    {
                        NotificationId = courseNotification.Id,
                        UserId = userId,
                        RoleName = null
                    }));
                }

                if (roleNames != null && roleNames.Any())
                {
                    recipients.AddRange(roleNames.Select(role => new NotificationRecipient
                    {
                        NotificationId = courseNotification.Id,
                        UserId = string.Empty,
                        RoleName = role
                    }));
                }

                if (recipients.Any())
                {
                    _context.NotificationRecipients.AddRange(recipients);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await _context.Database.RollbackTransactionAsync();
                throw new InvalidOperationException("Không thể lưu thông báo: " + ex.Message, ex);
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

        public async Task<bool> HasRecentNotification(NotificationType type, int courseId, int? days = 7)
        {
            var query = _context.Notifications
                .Where(n => n.Type == type && n.CourseId == courseId);

            if (days.HasValue && days.Value > 0)
            {
                var since = DateTime.UtcNow.AddDays(-days.Value);
                query = query.Where(n => n.SentAt >= since);
            }

            return await query.AnyAsync();
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
