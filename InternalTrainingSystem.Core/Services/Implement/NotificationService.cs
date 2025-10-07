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

        public async Task SaveNotificationAsync(CourseNotification courseNotification)
        {
            try
            {
                _context.CourseNotifications.Add(courseNotification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{ex.Message}", ex);
            }
        }

        public CourseNotification? GetNotificationByCourseAndType(int courseId, NotificationType type)
        {
            return _context.CourseNotifications
                .FirstOrDefault(n => n.CourseId == courseId && n.Type == type);
        }
    }
}
