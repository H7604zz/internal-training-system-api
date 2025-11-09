using Amazon.S3.Model;
using Amazon.S3;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Utils;

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
    IFormFile file,
    string subFolder,
    StorageObjectMetadata meta,
    CancellationToken ct = default)
        {
            var ext = Path.GetExtension(file.FileName);
            var key = $"{subFolder.TrimEnd('/')}/{Guid.NewGuid():N}{ext}";

            using var stream = file.OpenReadStream();

            var put = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = stream,
                ContentType = meta.ContentType, // e.g. "text/plain; charset=utf-8"
            };

            // Only if you actually gzip the payload:
            // if (meta.ContentEncoding == "gzip")
            //     put.Headers.ContentEncoding = "gzip";

            // Vietnamese filename displayed correctly
            put.Headers.ContentDisposition = meta.ContentDisposition;

            await _s3.PutObjectAsync(put, ct);

            var url = !string.IsNullOrWhiteSpace(_publicBaseUrl)
                ? $"{_publicBaseUrl!.TrimEnd('/')}/{key}"
                : $"https://{_bucket}.s3.amazonaws.com/{key}";

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
                Expires = DateTime.Now.Add(ttl),
                Verb = HttpVerb.GET
            };
            return _s3.GetPreSignedURL(req);
        }
    }
}
