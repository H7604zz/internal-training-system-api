using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _webAppBaseUrl;

        public CertificateRepository(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _webAppBaseUrl = config["ApplicationSettings:WebAppBaseUrl"] ?? "http://localhost:5143";
        }

        public async Task IssueCertificateAsync(string userId, int courseId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("Không tìm thấy học viên.");

            var course = await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                throw new Exception("Không tìm thấy khóa học.");

            // Kiểm tra enrollment
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
                throw new Exception("Học viên chưa được ghi danh vào khóa học này.");

            // Kiểm tra chứng chỉ đã tồn tại
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


            // Gửi email thông báo nhận chứng chỉ
            if (!string.IsNullOrEmpty(user.Email))
            {
                string viewCertificatesUrl = $"{_webAppBaseUrl}/khoa-hoc/chung-chi/{courseId}";
                string emailContent = $@"
                    Xin chào {user.FullName},<br/><br/>
                    Chúc mừng bạn đã <b>hoàn thành khóa học {course.CourseName}</b>! 🎉<br/><br/>
                    Hệ thống đã tự động cấp cho bạn chứng chỉ hoàn thành khóa học.<br/>
                    Bạn có thể xem hoặc tải chứng chỉ trong trang <a href='{viewCertificatesUrl}'>Chứng chỉ của tôi</a>.<br/><br/>
                    Trân trọng,<br/>
                    <b>Phòng Đào Tạo</b>
                ";

                Hangfire.BackgroundJob.Enqueue(() => EmailHelper.SendEmailAsync(
                    user.Email,
                    $"Chúc mừng bạn nhận chứng chỉ khóa học {course.CourseName}",
                    emailContent
                ));
            }
        }

        public async Task<CertificateResponse?> GetCertificateAsync(int courseId, string userId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserId == userId);

            if (certificate == null)
                return null;

            return new CertificateResponse
            {
                CertificateId = certificate.CertificateId,
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
