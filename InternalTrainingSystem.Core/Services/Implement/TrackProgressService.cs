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
        public Task<decimal> UpdateCourseProgressAsync(int courseId, CancellationToken ct = default)
        {
            return _trackProgressRepo.UpdateCourseProgressAsync(courseId, ct);
        }

        public Task<decimal> UpdateModuleProgressAsync(int moduleId, CancellationToken ct = default)
        {
            return _trackProgressRepo.UpdateModuleProgressAsync(moduleId, ct);
        }
    }
}
