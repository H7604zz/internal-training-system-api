using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Configuration
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        [Required]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        public int ExpireHours { get; set; } = 24;
    }

    public class EmailSettings
    {
        public const string SectionName = "EmailSettings";

        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class ExternalApiKeys
    {
        public const string SectionName = "ExternalApiKeys";

        public string GoogleApiKey { get; set; } = string.Empty;
        public string MicrosoftApiKey { get; set; } = string.Empty;
        public string SendGridApiKey { get; set; } = string.Empty;
        public string AzureStorageConnectionString { get; set; } = string.Empty;
    }

    public class FileUploadSettings
    {
        public const string SectionName = "FileUploadSettings";

        public int MaxFileSizeMB { get; set; } = 10;
        public List<string> AllowedFileExtensions { get; set; } = new();
    }

    public class ApplicationSettings
    {
        public const string SectionName = "ApplicationSettings";

        public string ApiBaseUrl { get; set; } = string.Empty;
        public List<string> AllowedOrigins { get; set; } = new();
    }
}