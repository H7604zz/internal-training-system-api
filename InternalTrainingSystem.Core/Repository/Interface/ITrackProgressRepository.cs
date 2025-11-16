using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ITrackProgressRepository
    {
        Task<decimal> UpdateModuleProgressAsync(string userId, int moduleId, CancellationToken ct = default);
        Task<decimal> UpdateCourseProgressAsync(string userId, int courseId, CancellationToken ct = default);
        Task<DepartmentDetailDto> TrackProgressDepartment(int departmentId);
        Task<ClassPassResultDto> GetClassPassRateAsync(int classId);
    }
}
