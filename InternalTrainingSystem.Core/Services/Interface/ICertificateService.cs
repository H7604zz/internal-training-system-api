using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICertificateService
    {
        Task IssueCertificateAsync(string userId, int courseId);
        Task<List<CertificateResponse>> GetCertificateByUserAsync(string userId);
        Task<CertificateResponse?> GetCertificateAsync(int courseId, string userId);
        Task<byte[]> GenerateCertificatePdfAsync(int courseId, string userId);
    }
}
