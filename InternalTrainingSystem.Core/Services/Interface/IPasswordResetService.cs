namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IPasswordResetService
    {
        Task<bool> SendPasswordResetOtpAsync(string email);
        Task<(bool Success, string? NewPassword)> ResetPasswordWithOtpAsync(string email, string otp);
        string GenerateRandomPassword(int length = 8);
    }
}