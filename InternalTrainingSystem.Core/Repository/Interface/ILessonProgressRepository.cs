using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ILessonProgressRepository
    {
        Task<LessonProgress?> GetAsync(string userId, int lessonId, CancellationToken ct = default);
        Task EnsureStartedAsync(string userId, int lessonId, CancellationToken ct = default);
        Task MarkDoneAsync(string userId, int lessonId, CancellationToken ct = default);

        Task<bool> IsEnrolledAsync(int courseId, string userId, CancellationToken ct = default);

        Task<Course?> GetCourseWithStructureAsync(int courseId, CancellationToken ct = default); // include Modules->Lessons

        Task<Lesson?> GetLessonWithModuleCourseAsync(int lessonId, CancellationToken ct = default);

        Task<LessonProgress?> GetProgressAsync(string userId, int lessonId, CancellationToken ct = default);

        Task UpsertDoneAsync(string userId, int lessonId, bool done, CancellationToken ct = default);

        Task<Dictionary<int, LessonProgress>> GetProgressMapAsync(string userId, IEnumerable<int> lessonIds, CancellationToken ct = default);

        Task<int> CountCourseCompletedLessonsAsync(string userId, int courseId, CancellationToken ct = default);

        Task<int> CountCourseTotalLessonsAsync(int courseId, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
        Task<bool> HasUserPassedQuizAsync(int quizId, string userId, CancellationToken ct = default);
    }
}
