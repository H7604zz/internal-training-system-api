using Amazon.S3.Model;
using Amazon.S3;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class S3FileStorage : IFileStorage
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;
        private readonly string? _publicBaseUrl;
        private readonly IHttpContextAccessor _http;

        public S3FileStorage(IConfiguration config, IHttpContextAccessor http)
        {
            _http = http;
            _bucket = config["AWS_S3_BUCKET"] ?? throw new ArgumentNullException("AWS_S3_BUCKET");
            var region = config["AWS_S3_REGION"] ?? throw new ArgumentNullException("AWS_S3_REGION");
            _publicBaseUrl = config["AWS_S3_PUBLIC_BASE_URL"]?.TrimEnd('/');

            _s3 = new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(region));
        }

        public async Task<(string url, string relativePath)> SaveAsync(
            IFormFile file, string subFolder, CancellationToken ct = default)
        {
            // Normalize key
            var ext = Path.GetExtension(file.FileName);
            var name = Path.GetFileNameWithoutExtension(file.FileName);
            var safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
            var key = $"{subFolder.Trim('/')}/{safeName}_{Guid.NewGuid():N}{ext}".Replace("\\", "/");

            // Upload
            using var stream = file.OpenReadStream();
            var put = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = stream,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                CannedACL = S3CannedACL.PublicRead  
            };
            var resp = await _s3.PutObjectAsync(put, ct);
            if ((int)resp.HttpStatusCode >= 300)
                throw new InvalidOperationException($"S3 upload failed ({resp.HttpStatusCode}).");

            // Build URL:
            // - If bucket/objects are public and you set AWS_S3_PUBLIC_BASE_URL → return nice HTTPS URL
            // - Else return an s3:// style path; your app can generate pre-signed URLs later if needed
            var url = _publicBaseUrl is not null
                ? $"{_publicBaseUrl}/{key}"
                : $"s3://{_bucket}/{key}";

            // relativePath = key for DB (used for delete)
            return (url, key);
        }

        public async Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default)
        {
            var del = new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = relativePath
            };
            var resp = await _s3.DeleteObjectAsync(del, ct);
            return (int)resp.HttpStatusCode < 300;
        }

        // Optional: generate a time-limited URL for private buckets
        public string GetReadUrl(string key, TimeSpan ttl)
        {
            var req = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Expires = DateTime.UtcNow.Add(ttl),
                Verb = HttpVerb.GET
            };
            return _s3.GetPreSignedURL(req);
        }
    }
}
