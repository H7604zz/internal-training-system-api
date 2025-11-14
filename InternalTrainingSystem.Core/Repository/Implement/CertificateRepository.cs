using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CertificateResponse> IssueCertificateAsync(string userId, int courseId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("Không tìm thấy học viên.");

            bool allSessionsCompletedForUser = await _context.ScheduleParticipants
                 .Where(sp => sp.UserId == userId && sp.Schedule.CourseId == courseId)
                 .AllAsync(sp => sp.Status == ScheduleConstants.ParticipantStatus.Completed);
            if (!allSessionsCompletedForUser)
                throw new Exception("Học viên chưa hoàn thành toàn bộ buổi học, không thể cấp chứng chỉ.");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                throw new Exception("Không tìm thấy khóa học.");

            var existing = await _context.Certificates
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);
            if (existing != null)
                throw new Exception("Học viên đã có chứng chỉ cho khóa học này.");

            var certificate = new Certificate
            {
                UserId = userId,
                CourseId = courseId,
                CertificateName = $"Chứng chỉ hoàn thành khóa học: {course.CourseName}",
                IssueDate = DateTime.Now,
                ExpirationDate = null,
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return new CertificateResponse
            {
                CertificateId = certificate.CertificateId,
                CourseName = course.CourseName,
                CourseCode = course.Code!,
                CertificateName = certificate.CertificateName,
                IssueDate = certificate.IssueDate,
                ExpirationDate = certificate.ExpirationDate,
                FullName = user.FullName,
            };
        }

        public async Task<CertificateResponse?> GetCertificateByIdAsync(int id, string userId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CertificateId == id && c.UserId == userId);

            return new CertificateResponse
            {
                CertificateId = certificate!.CertificateId,
                CourseName = certificate.Course.CourseName,
                CourseCode = certificate.Course.Code!,
                CertificateName = certificate.CertificateName,
                IssueDate = certificate.IssueDate,
                ExpirationDate = certificate.ExpirationDate,
                FullName = certificate.User.FullName,
            };
        }

        public async Task<List<CertificateResponse>> GetCertificateByUserAsync(string userId)
        {
            var certificates = await _context.Certificates
            .Include(c => c.Course)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.IssueDate)
            .Select(c => new CertificateResponse
            {
                CertificateId = c.CertificateId,
                CertificateName = c.CertificateName,
                CourseCode = c.Course.Code!,
                CourseName = c.Course.CourseName,
                IssueDate = c.IssueDate,
                ExpirationDate = c.ExpirationDate
            })
            .ToListAsync();

            return certificates;
        }
    }
}
