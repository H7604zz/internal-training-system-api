using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepo;

        public CertificateService(ICertificateRepository certificateRepo)
        {
            _certificateRepo = certificateRepo;
        }

        public async Task<CertificateResponse?> GetCertificateByIdAsync(int id, string userId)
        {
            return await _certificateRepo.GetCertificateByIdAsync(id, userId);
        }

        public async Task<List<CertificateResponse>> GetCertificateByUserAsync(string userId)
        {
            return await _certificateRepo.GetCertificateByUserAsync(userId);
        }

        public async Task<CertificateResponse> IssueCertificateAsync(string userId, int courseId)
        {
            return await _certificateRepo.IssueCertificateAsync(userId, courseId);
        }
    }
}
