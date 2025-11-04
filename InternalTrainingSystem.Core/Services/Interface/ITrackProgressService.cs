using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ITrackProgressService
    {
        Task<decimal> UpdateModuleProgressAsync(string userId, int moduleId, CancellationToken ct = default);
        Task<decimal> UpdateCourseProgressAsync(string userId, int courseId, CancellationToken ct = default);
        Task<DepartmentDetailDto> TrackProgressDepartment(int departmentId);
    }
}
