using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;

        public LocalFileStorage(IWebHostEnvironment env, IHttpContextAccessor http)
        {
            _env = env;
            _http = http;
        }

        public async Task<(string url, string relativePath)> SaveAsync(
            IFormFile file, string subFolder, CancellationToken ct = default)
        {
            var wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
            var folder = Path.Combine(wwwroot, subFolder);
            Directory.CreateDirectory(folder);

            var safeName = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            var filename = $"{safeName}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, filename);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            // relative path (for delete) and public URL:
            var relativePath = Path.Combine(subFolder, filename).Replace("\\", "/");
            var req = _http.HttpContext?.Request;
            // build absolute url: http(s)://host/uploads/lessons/...
            var baseUrl = req is null
                ? ""
                : $"{req.Scheme}://{req.Host}";
            var url = $"{baseUrl}/{relativePath}";
            return (url, relativePath);
        }

        public Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default)
        {
            var full = Path.Combine(_env.ContentRootPath, "wwwroot", relativePath);
            if (!File.Exists(full)) return Task.FromResult(false);
            File.Delete(full);
            return Task.FromResult(true);
        }
    }
}
