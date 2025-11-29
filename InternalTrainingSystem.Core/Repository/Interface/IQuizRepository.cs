using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IQuizRepository
    {
        Task<Quiz?> GetActiveQuizWithQuestionsAsync(int quizId, CancellationToken ct = default);
        Task<QuizDetailDto2?> GetDetailAsync(int quizId, CancellationToken ct);
        Task<Quiz?> GetActiveQuizAsync(int quizId, CancellationToken ct = default);
    }
}
