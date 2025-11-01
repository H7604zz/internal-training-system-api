namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ITrackProgressRepository
    {
        Task<decimal> UpdateModuleProgressAsync(int moduleId, CancellationToken ct = default);
        Task<decimal> UpdateCourseProgressAsync(int courseId, CancellationToken ct = default);
    }
}
