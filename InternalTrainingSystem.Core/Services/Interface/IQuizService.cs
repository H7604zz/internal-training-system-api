using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IQuizService
    {
        Task<QuizDetailDto?> GetQuizForAttemptAsync(int quizId,int attemptId,string userId,bool shuffleQuestions = false,bool shuffleAnswers = false,CancellationToken ct = default); 
        Task<StartQuizResponse> StartAttemptAsync(int quizId, string userId, CancellationToken ct = default);
        Task<AttemptResultDto> SubmitAttemptAsync(int attemptId, string userId, SubmitAttemptRequest req, CancellationToken ct = default);
        Task<AttemptResultDto> GetAttemptResultAsync(int attemptId, string userId, CancellationToken ct = default);
        Task<PagedResult<AttemptHistoryItem>> GetAttemptHistoryAsync(int quizId, string userId, int page, int pageSize, CancellationToken ct = default);
        Task<StartQuizResponse> StartAttemptByLessonAsync(int lessonId, string userId, CancellationToken ct = default);
        Task<AttemptResultDto> SubmitAttemptByLessonAsync(int lessonId, int attemptId, string userId, SubmitAttemptRequest req, CancellationToken ct = default);
        Task<QuizDetailDto2> GetDetailAsync(int quizId, CancellationToken ct);
        Task<QuizInfoDto?> GetQuizInfoAsync(int quizId, string userId, CancellationToken ct = default);
    }
}
