using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface INotificationService
    {
        Task SaveNotificationAsync(CourseNotification courseNotification);
        CourseNotification? GetNotificationByCourseAndType(int courseId, NotificationType type);
    }
}
