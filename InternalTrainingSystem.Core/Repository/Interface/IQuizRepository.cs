namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IQuizRepository
    {
        Task<bool> CheckQuizPassedAsync(int quizId);
    }
}
