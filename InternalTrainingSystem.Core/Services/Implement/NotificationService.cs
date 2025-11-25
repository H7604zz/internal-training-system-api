using InternalTrainingSystem.Core.Common.Constants;
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
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;

        public NotificationService(INotificationRepository notificationRepo, IUserRepository userRepository,
            ICourseRepository courseRepository)
        {
            _notificationRepo = notificationRepo;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
        }

        public async Task SaveNotificationAsync(Notification notification, List<string>? userIds = null, List<string>? roleNames = null)
        {
            await _notificationRepo.SaveNotificationAsync(notification, userIds, roleNames);
        }

        public Notification? GetNotificationByCourseAndType(int courseId, NotificationType type)
        {
            return _notificationRepo.GetNotificationByCourseAndType(courseId, type);
        }

        public Notification? GetNotificationByClassAndType(int classId, NotificationType type)
        {
            return _notificationRepo.GetNotificationByClassAndType(classId, type);
        }

        public async Task<bool> HasRecentNotification(NotificationType type, int courseId, int? days = 7)
        {
            return await _notificationRepo.HasRecentNotification(type, courseId, days);
        }

        public async Task DeleteOldNotificationsAsync(int courseId, NotificationType type)
        {
           await _notificationRepo.DeleteOldNotificationsAsync(courseId, type);
        }

        public async Task<List<Notification>> GetNotificationsAsync(string? userId = null, string? roleName = null)
        {
           return await _notificationRepo.GetNotificationsAsync(userId, roleName);
        }

        public async Task NotifyTrainingDepartmentAsync(int courseId)
        {
            var trainingUsers = await _userRepository.GetUsersByRoleAsync(UserRoles.TrainingDepartment);
            if (trainingUsers.Count == 0)
                return;
            var course = await _courseRepository.GetCourseByCourseIdAsync(courseId);
            bool isApproved = course.Status.Equals(
                CourseConstants.Status.Approve, StringComparison.OrdinalIgnoreCase);

            var notification = new Notification
            {
                CourseId = course.CourseId,
                Type = isApproved
                    ? NotificationType.CourseApproved
                    : NotificationType.CourseRejected,
                Message = isApproved
                    ? $"Khóa học \"{course.CourseName}\" đã được giám đốc phê duyệt."
                    : $"Khóa học \"{course.CourseName}\" đã bị từ chối. Lý do: {course.RejectionReason}",
                SentAt = DateTime.Now,
                EntityType = "Course",
                EntityId = course.CourseId
            };

            await _notificationRepo.SaveNotificationAsync(notification, trainingUsers.Select(u => u.Id).ToList()
            );
        }
    }
}
