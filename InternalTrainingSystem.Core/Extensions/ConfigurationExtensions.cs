namespace InternalTrainingSystem.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Load environment variables from .env file
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="envFilePath">Path to .env file (optional, defaults to .env in root)</param>
        public static void LoadEnvironmentVariables(this IConfigurationBuilder configuration, string? envFilePath = null)
        {
            var envPath = envFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), ".env");
            
            if (File.Exists(envPath))
            {
                var envVars = new Dictionary<string, string?>();
                
                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmedLine = line.Trim();
                    
                    // Skip empty lines and comments
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                        continue;
                    
                    var separatorIndex = trimmedLine.IndexOf('=');
                    if (separatorIndex <= 0) continue;
                    
                    var key = trimmedLine[..separatorIndex].Trim();
                    var value = trimmedLine[(separatorIndex + 1)..].Trim();
                    
                    // Remove quotes if present
                    if (value.StartsWith('"') && value.EndsWith('"'))
                        value = value[1..^1];
                    
                    // Set both environment variable and configuration
                    Environment.SetEnvironmentVariable(key, value);
                    
                    // Map CONNECTION_STRING to proper configuration key
                    switch (key)
                    {
                        case "CONNECTION_STRING":
                            envVars["ConnectionStrings:DefaultConnection"] = value;
                            break;

                        // Email settings
                        case "SMTP_SERVER":
                            envVars["EmailSettings:SmtpServer"] = value;
                            break;
                        case "SMTP_PORT":
                            envVars["EmailSettings:SmtpPort"] = value;
                            break;
                        case "SMTP_USERNAME":
                            envVars["EmailSettings:SmtpUsername"] = value;
                            break;
                        case "SMTP_PASSWORD":
                            envVars["EmailSettings:SmtpPassword"] = value;
                            break;
                        case "FROM_EMAIL":
                            envVars["EmailSettings:FromEmail"] = value;
                            break;
                        case "FROM_NAME":
                            envVars["EmailSettings:FromName"] = value;
                            break;
                        //AWS S3 settings
                        case "STORAGE_PROVIDER": 
                            envVars["STORAGE_PROVIDER"] = value; 
                            break;
                        case "AWS_S3_BUCKET": 
                            envVars["AWS_S3_BUCKET"] = value; 
                            break;
                        case "AWS_S3_REGION": 
                            envVars["AWS_S3_REGION"] = value; 
                            break;
                        case "AWS_S3_PUBLIC_BASE_URL": 
                            envVars["AWS_S3_PUBLIC_BASE_URL"] = value; 
                            break;
                        default:
                            envVars[key] = value;
                            break;
                    }
                }
                
                configuration.AddInMemoryCollection(envVars);
                
                Console.WriteLine($"Loaded {envVars.Count} environment variables from {envPath}");
            }
            else
            {
                Console.WriteLine($"Environment file not found at: {envPath}");
            }
        }

        /// <summary>
        /// Override configuration with environment variables
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public static void OverrideWithEnvironmentVariables(this IServiceCollection services, IConfiguration configuration)
        {
            // Override connection string
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
                ?? configuration.GetConnectionString("DefaultConnection");
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                configuration["ConnectionStrings:DefaultConnection"] = connectionString;
            }

            // Override JWT settings
            OverrideIfNotEmpty("JWT_SECRET_KEY", "JwtSettings:SecretKey");
            OverrideIfNotEmpty("JWT_ISSUER", "JwtSettings:Issuer");
            OverrideIfNotEmpty("JWT_AUDIENCE", "JwtSettings:Audience");
            OverrideIfNotEmpty("JWT_EXPIRE_HOURS", "JwtSettings:ExpireHours");

            // Override external API keys
            OverrideIfNotEmpty("GOOGLE_API_KEY", "ExternalApiKeys:GoogleApiKey");
            OverrideIfNotEmpty("MICROSOFT_API_KEY", "ExternalApiKeys:MicrosoftApiKey");
            OverrideIfNotEmpty("SENDGRID_API_KEY", "ExternalApiKeys:SendGridApiKey");
            OverrideIfNotEmpty("AZURE_STORAGE_CONNECTION_STRING", "ExternalApiKeys:AzureStorageConnectionString");

            // Override email settings
            OverrideIfNotEmpty("SMTP_SERVER", "EmailSettings:SmtpServer");
            OverrideIfNotEmpty("SMTP_PORT", "EmailSettings:SmtpPort");
            OverrideIfNotEmpty("SMTP_USERNAME", "EmailSettings:SmtpUsername");
            OverrideIfNotEmpty("SMTP_PASSWORD", "EmailSettings:SmtpPassword");
            OverrideIfNotEmpty("FROM_EMAIL", "EmailSettings:FromEmail");
            OverrideIfNotEmpty("FROM_NAME", "EmailSettings:FromName");

            // Override file upload settings
            OverrideIfNotEmpty("MAX_FILE_SIZE_MB", "FileUploadSettings:MaxFileSizeMB");

            var allowedExtensions = Environment.GetEnvironmentVariable("ALLOWED_FILE_EXTENSIONS");
            if (!string.IsNullOrEmpty(allowedExtensions))
            {
                var extensions = allowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < extensions.Length; i++)
                {
                    configuration[$"FileUploadSettings:AllowedFileExtensions:{i}"] = extensions[i].Trim();
                }
            }

            // Override application settings
            OverrideIfNotEmpty("API_BASE_URL", "ApplicationSettings:ApiBaseUrl");
            
            var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
            if (!string.IsNullOrEmpty(allowedOrigins))
            {
                var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < origins.Length; i++)
                {
                    configuration[$"ApplicationSettings:AllowedOrigins:{i}"] = origins[i].Trim();
                }
            }
            // Override AWS S3 settings
            OverrideIfNotEmpty("STORAGE_PROVIDER", "STORAGE_PROVIDER");
            OverrideIfNotEmpty("AWS_S3_BUCKET", "AWS_S3_BUCKET");
            OverrideIfNotEmpty("AWS_S3_REGION", "AWS_S3_REGION");
            OverrideIfNotEmpty("AWS_S3_PUBLIC_BASE_URL", "AWS_S3_PUBLIC_BASE_URL");


            void OverrideIfNotEmpty(string envKey, string configKey)
            {
                var envValue = Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrEmpty(envValue))
                {
                    configuration[configKey] = envValue;
                }
            }
        }
    }
}