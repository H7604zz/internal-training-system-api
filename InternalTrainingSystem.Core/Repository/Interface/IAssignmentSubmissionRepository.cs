using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IAssignmentSubmissionRepository
    {
        Task<AssignmentSubmission?> GetByIdWithUserAsync(int submissionId, CancellationToken ct = default);
        Task<List<AssignmentSubmission>> GetByAssignmentAsync(int assignmentId, CancellationToken ct = default);
        Task AddAsync(AssignmentSubmission entity, CancellationToken ct = default);
        void Update(AssignmentSubmission entity);
        Task<AssignmentSubmission?> GetByAssignmentAndUserSingleAsync(int assignmentId, string userId, CancellationToken ct);
    }
}
