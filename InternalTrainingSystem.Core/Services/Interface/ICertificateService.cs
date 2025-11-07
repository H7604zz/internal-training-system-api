using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICertificateService
    {
        Task<CertificateResponse> IssueCertificateAsync(string userId, int courseId);
        Task<List<CertificateResponse>> GetCertificateByUserAsync(string userId);
        Task<CertificateResponse?> GetCertificateByIdAsync(int id, string userId);
    }
}
