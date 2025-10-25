using InternalTrainingSystem.Core.Services.Interface;
using System.Net;
using System.Net.Mail;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string otp)
        {
            try
            {
                var subject = "Password Reset Request - Internal Training System";
                var body = $@"
                    <html>
                    <body>
                        <h2>Password Reset Request</h2>
                        <p>Dear {fullName},</p>
                        <p>You have requested to reset your password. Please use the following OTP code:</p>
                        <h3 style='color: #007bff; font-size: 24px; letter-spacing: 2px;'>{otp}</h3>
                        <p>This OTP is valid for 15 minutes.</p>
                        <p>If you did not request this password reset, please ignore this email.</p>
                        <br>
                        <p>Best regards,<br>Internal Training System Team</p>
                    </body>
                    </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendNewPasswordEmailAsync(string email, string fullName, string newPassword)
        {
            try
            {
                var subject = "Your New Password - Internal Training System";
                var body = $@"
                    <html>
                    <body>
                        <h2>Password Reset Successful</h2>
                        <p>Dear {fullName},</p>
                        <p>Your password has been successfully reset. Here is your new temporary password:</p>
                        <h3 style='color: #28a745; font-size: 20px; letter-spacing: 1px;'>{newPassword}</h3>
                        <p style='color: #dc3545;'><strong>Important:</strong> Please change this password after logging in for security purposes.</p>
                        <br>
                        <p>Best regards,<br>Internal Training System Team</p>
                    </body>
                    </html>";

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new password email to {Email}", email);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUsername;

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email configuration is missing. Skipping email send.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }
    }
}