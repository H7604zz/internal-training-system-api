namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ITrackProgressService
    {
        Task<decimal> UpdateModuleProgressAsync(int moduleId, CancellationToken ct = default);
        Task<decimal> UpdateCourseProgressAsync(int courseId, CancellationToken ct = default);
    }
}
