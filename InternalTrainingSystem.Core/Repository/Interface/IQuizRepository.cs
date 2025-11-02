using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IQuizRepository
    {
        Task<Quiz?> GetActiveQuizWithQuestionsAsync(int quizId, CancellationToken ct = default);
        Task<int> GetQuizMaxScoreAsync(int quizId, CancellationToken ct = default);
    }
}
