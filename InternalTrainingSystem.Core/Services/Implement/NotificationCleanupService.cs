
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class NotificationCleanupService : BackgroundService
    {

        private readonly IServiceProvider _serviceProvider;

        public NotificationCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanOldNotificationsAsync();
                await CleanCourseNotificationsAsync();

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private async Task CleanOldNotificationsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var cutoffDate = DateTime.Now.AddDays(-14);

                var oldNotifications = await context.Notifications
                    .Where(n => n.SentAt < cutoffDate && n.Recipients.Any())
                    .ToListAsync();

                if (oldNotifications.Any())
                {
                    context.Notifications.RemoveRange(oldNotifications);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"Đã xóa {oldNotifications.Count} thông báo cũ hơn 14 ngày ({DateTime.Now}).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa thông báo cũ: {ex.Message}");
            }
        }

        private async Task CleanCourseNotificationsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var completedCourseIds = await context.Courses
                    .Where(c => c.Classes.Any())
                    .Where(c => c.Classes.All(cl => cl.Status == ClassConstants.Status.Completed))
                    .Select(c => c.CourseId)
                    .ToListAsync();

                if (!completedCourseIds.Any())
                {
                    Console.WriteLine("Không có khóa học nào hoàn thành để xóa thông báo.");
                    return;
                }

                var relatedNotifications = await context.Notifications
                    .Include(n => n.Recipients)
                    .Where(n => n.CourseId != null && completedCourseIds.Contains(n.CourseId.Value))
                    .ToListAsync();

                if (!relatedNotifications.Any())
                {
                    Console.WriteLine("Không có thông báo nào liên quan đến các khóa học đã hoàn thành.");
                    return;
                }

                var allRecipients = relatedNotifications.SelectMany(n => n.Recipients).ToList();
                if (allRecipients.Any())
                {
                    context.RemoveRange(allRecipients);
                }

                context.Notifications.RemoveRange(relatedNotifications);
                await context.SaveChangesAsync();

                Console.WriteLine($"Đã xóa {relatedNotifications.Count} thông báo liên quan đến {completedCourseIds.Count} khóa học đã hoàn thành ({DateTime.Now}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa thông báo khóa học hoàn thành: {ex.Message}");
            }
        }

    }
}
