using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class TrackProgressService : ITrackProgressService
    {
        private readonly ITrackProgressRepository _trackProgressRepo;

        public TrackProgressService(ITrackProgressRepository trackProgressRepo)
        {
            _trackProgressRepo = trackProgressRepo;
        }
        public Task<decimal> UpdateCourseProgressAsync(string userId, int courseId, CancellationToken ct = default)
        {
            return _trackProgressRepo.UpdateCourseProgressAsync(userId, courseId, ct);
        }

        public Task<decimal> UpdateModuleProgressAsync(string userId, int moduleId, CancellationToken ct = default)
        {
            return _trackProgressRepo.UpdateModuleProgressAsync(userId, moduleId, ct);
        }
        public async Task<DepartmentDetailDto> TrackProgressDepartment(int departmentId)
        {
            return await _trackProgressRepo.TrackProgressDepartment(departmentId);
        }
        public async Task<ClassPassResultDto> GetClassPassRateAsync(int classId)
        {
            return await _trackProgressRepo.GetClassPassRateAsync(classId);
        }
    }
}
