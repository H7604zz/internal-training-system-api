using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IQuizAttemptRepository
    {
        Task<int> CountAttemptsAsync(int quizId, string userId, CancellationToken ct = default);
        Task<QuizAttempt> AddAttemptAsync(QuizAttempt attempt, CancellationToken ct = default);
        Task<QuizAttempt?> GetAttemptAsync(int attemptId, string userId, CancellationToken ct = default);
        Task<List<QuizAttempt>> GetUserAttemptsAsync(int quizId, string userId, CancellationToken ct = default);
        Task UpdateStatusAsync(int attemptId, string status, CancellationToken ct = default);
        Task<int> CountAttemptsTodayAsync(int quizId,string userId,DateTime from,DateTime to,CancellationToken ct);
    }
}
