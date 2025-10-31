using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class LessonProgressesRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly QuizRepository _quiz;

        public LessonProgressesRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context, QuizRepository quiz)
        {
            _context = context;
            _userManager = userManager;
            _quiz = quiz;
        }

        /// <summary>
        /// Kiểm tra lesson đã hoàn thành chưa:
        /// - Nếu có quiz: quiz phải pass (>=80)
        /// - Nếu không có quiz: IsDone = true
        /// </summary>
        public async Task<bool> CheckLessonPassedAsync(string userId, int lessonId, CancellationToken ct = default)
        {
            // 1️⃣ Lấy lesson và quiz kèm theo
            var lesson = await _context.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId, ct);

            if (lesson == null)
                throw new InvalidOperationException("Lesson not found.");

            // 2️⃣ Lấy tiến độ bài học
            var progress = await _context.LessonProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

            bool isDone = progress?.IsDone ?? false;

            // 3️⃣ Nếu lesson KHÔNG có quiz → chỉ cần IsDone = true
            if (lesson.QuizId == null)
                return isDone;

            // 4️⃣ Nếu lesson có quiz → kiểm tra điểm quiz
            bool quizPassed = await _quiz.CheckQuizPassedAsync(lesson.QuizId.Value);

            // 5️⃣ Kết hợp điều kiện: quizPassed + IsDone
            return quizPassed && isDone;
        }


    }
}
