
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
                    .Where(n => n.CreatedAt < cutoffDate)
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
    }
}
