using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CourseHistoryRepository : ICourseHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CourseHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseHistory>> GetCourseHistoriesAsync()
        {
            return await _context.CourseHistories
                .Include(h => h.User) // để lấy thông tin người thực hiện
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();
        }

    }
}
