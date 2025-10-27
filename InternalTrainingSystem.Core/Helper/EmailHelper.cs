using System.Net;
using System.Net.Mail;

namespace InternalTrainingSystem.Core.Helper
{
    public static class EmailHelper
    {
        private static string _smtpServer;
        private static int _smtpPort;
        private static string _fromEmail;
        private static string _fromName;
        private static string _smtpPassword;

        public static void Configure(string smtpServer, int smtpPort, string fromEmail, string fromName, string smtpPassword)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _fromEmail = fromEmail;
            _fromName = fromName;
            _smtpPassword = smtpPassword;
        }

        public static async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_smtpServer))
                throw new InvalidOperationException();

            try
            {
                using var smtpClient = new SmtpClient(_smtpServer)
                {
                    Port = _smtpPort,
                    Credentials = new NetworkCredential(_fromEmail, _smtpPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không gửi được mail: {ex.Message}", ex);
            }
        }
    }
}
