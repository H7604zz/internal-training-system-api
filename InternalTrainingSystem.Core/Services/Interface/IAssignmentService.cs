using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IAssignmentService
    {
        Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentForm form, string mentorId, CancellationToken ct);
        Task<AssignmentDto> UpdateAssignmentAsync(int assignmentId, UpdateAssignmentForm form, string mentorId, CancellationToken ct);
        Task DeleteAssignmentAsync(int assignmentId, string mentorId, CancellationToken ct);
        Task<AssignmentDto?> GetAssignmentForClassAsync(int classId,CancellationToken ct);
        Task<AssignmentDto?> GetAssignmentForStaffInClassAsync(int classId,string userId,CancellationToken ct);
        Task<AssignmentDto?> GetAssignmentByIdAsync(int assignmentId, CancellationToken ct);
        Task<AssignmentDto?> GetAssignmentForStaffAsync(int assignmentId, string userId, CancellationToken ct);
        Task<List<AssignmentSubmissionSummaryDto>> GetSubmissionsForAssignmentAsync(
            int assignmentId, string mentorId, CancellationToken ct);
        Task<AssignmentSubmissionDetailDto?> GetSubmissionDetailAsync(
            int submissionId, string requesterId, bool isMentor, CancellationToken ct);
        // CHỈ 1 FILE cho mỗi submission
        Task<AssignmentSubmissionDetailDto> CreateSubmissionAsync(
            int assignmentId,
            string userId,
            CreateSubmissionRequest request,
            (string fileName, string relativePath, string url, string? mimeType, long? sizeBytes)? file,
            CancellationToken ct);

    }
}
