using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ISubmissionFileRepository
    {
        Task AddRangeAsync(IEnumerable<SubmissionFile> files, CancellationToken ct = default);
        Task RemoveBySubmissionIdAsync(int submissionId, CancellationToken ct = default);
    }
}
