namespace InternalTrainingSystem.Core.Utils
{
    public sealed class StorageObjectMetadata
    {
        public string ContentType { get; private set; }          // includes charset for text/*
        public string ContentDisposition { get; private set; }   // filename*=UTF-8''...

        private StorageObjectMetadata(string contentType, string contentDisposition)
        {
            ContentType = contentType;
            ContentDisposition = contentDisposition;
        }

        public static StorageObjectMetadata ForUpload(string originalFileName, string inputContentType)
        {
            var fileNameEscaped = Uri.EscapeDataString(originalFileName);

            // If it's text, append charset=utf-8. Do NOT set Content-Encoding for UTF-8.
            var ct = NormalizeContentType(inputContentType);
            if (IsTextMime(ct) && !ct.Contains("charset=", StringComparison.OrdinalIgnoreCase))
                ct = $"{ct}; charset=utf-8";

            var disposition = $"inline; filename*=UTF-8''{fileNameEscaped}";

            return new StorageObjectMetadata(ct, disposition);
        }

        private static string NormalizeContentType(string? ct) => (ct ?? "application/octet-stream").Trim();

        private static bool IsTextMime(string ct)
        {
            ct = ct.ToLowerInvariant();
            if (ct.StartsWith("text/")) return true;
            return ct is "application/json" or "application/xml" or "application/javascript"
                        or "application/x-javascript" or "application/csv" or "text/csv";
        }
    }
}
