namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IFileStorage
    {
        Task<(string url, string relativePath)> SaveAsync(
            IFormFile file,
            string subFolder,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default);
    }
}
