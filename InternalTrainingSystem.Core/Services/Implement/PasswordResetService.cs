using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<PasswordResetService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetOtpAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return false;
                }

                // Generate 6-digit OTP
                var otp = GenerateOtp();

                // Store OTP in user record with expiry (15 minutes)
                user.PasswordResetToken = otp;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return false;
                }

                // Send email with OTP
                var emailSent = await _emailService.SendPasswordResetEmailAsync(email, user.FullName, otp);
                if (!emailSent)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<(bool Success, string? NewPassword)> ResetPasswordWithOtpAsync(string email, string otp)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return (false, null);
                }

                // Validate OTP
                if (string.IsNullOrEmpty(user.PasswordResetToken) ||
                    user.PasswordResetToken != otp ||
                    user.PasswordResetTokenExpiry == null ||
                    user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return (false, null);
                }

                // Generate new random password
                var newPassword = GenerateRandomPassword();

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (!result.Succeeded)
                {
                    return (false, null);
                }

                // Clear reset token
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _userManager.UpdateAsync(user);

                // Send new password via email
                var emailSent = await _emailService.SendNewPasswordEmailAsync(email, user.FullName, newPassword);
                if (!emailSent)
                {
                    return (false, null);
                }

                return (true, newPassword);
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }

        public string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            var result = new StringBuilder();
            using var rng = RandomNumberGenerator.Create();

            // Ensure at least one character from each category
            var categories = new[]
            {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ", // Uppercase
                "abcdefghijkmnopqrstuvwxyz", // Lowercase  
                "0123456789", // Numbers
                "!@#$%^&*" // Special characters
            };

            // Add one character from each category
            foreach (var category in categories)
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var randomIndex = BitConverter.ToUInt32(bytes, 0) % category.Length;
                result.Append(category[(int)randomIndex]);
            }

            // Fill remaining length with random characters
            for (int i = result.Length; i < length; i++)
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var randomIndex = BitConverter.ToUInt32(bytes, 0) % validChars.Length;
                result.Append(validChars[(int)randomIndex]);
            }

            // Shuffle the result
            var chars = result.ToString().ToCharArray();
            for (int i = chars.Length - 1; i > 0; i--)
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var j = (int)(BitConverter.ToUInt32(bytes, 0) % (i + 1));
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }

        private string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (randomNumber % 900000 + 100000).ToString(); // 6-digit OTP
        }
    }
}