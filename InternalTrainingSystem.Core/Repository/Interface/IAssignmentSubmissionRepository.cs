using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IAssignmentSubmissionRepository
    {
        Task<AssignmentSubmission?> GetByIdWithUserAsync(int submissionId, CancellationToken ct = default);

        Task<List<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId, CancellationToken ct = default);

        Task<List<AssignmentSubmission>> GetByAssignmentAndUserAsync(
            int assignmentId,
            string userId,
            CancellationToken ct = default);

        Task<int> GetMaxAttemptNumberAsync(int assignmentId, string userId, CancellationToken ct = default);

        Task SetAllOldSubmissionsNotMain(int assignmentId, string userId, CancellationToken ct = default);

        Task AddAsync(AssignmentSubmission entity, CancellationToken ct = default);

        void Update(AssignmentSubmission entity);
    }
}
