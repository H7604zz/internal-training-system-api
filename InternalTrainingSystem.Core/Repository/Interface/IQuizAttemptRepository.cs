using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IQuizAttemptRepository
    {
        Task<int> CountAttemptsAsync(int quizId, string userId, CancellationToken ct = default);
        Task<QuizAttempt> AddAttemptAsync(QuizAttempt attempt, CancellationToken ct = default);
        Task<QuizAttempt?> GetAttemptAsync(int attemptId, string userId, CancellationToken ct = default);
        Task<(IReadOnlyList<QuizAttempt> items, int total)> GetAttemptHistoryAsync(
            int quizId, string userId, int page, int pageSize, CancellationToken ct = default);
    }
}
