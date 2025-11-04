using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IUserAnswerRepository
    {
        Task AddAsync(UserAnswer entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<UserAnswer> entities, CancellationToken ct = default);
        Task<IReadOnlyList<UserAnswer>> GetByAttemptAsync(int attemptId, CancellationToken ct = default);
    }
}
