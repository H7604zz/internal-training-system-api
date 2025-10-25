namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string otp);
        Task<bool> SendNewPasswordEmailAsync(string email, string fullName, string newPassword);
    }
}