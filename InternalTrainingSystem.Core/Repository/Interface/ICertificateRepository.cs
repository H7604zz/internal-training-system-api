using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ICertificateRepository
    {
        Task IssueCertificateAsync(string userId, int courseId);
        Task<List<CertificateResponse>> GetCertificateByUserAsync(string userId);
        Task<CertificateResponse?> GetCertificateByIdAsync(int id, string userId);
    }
}
