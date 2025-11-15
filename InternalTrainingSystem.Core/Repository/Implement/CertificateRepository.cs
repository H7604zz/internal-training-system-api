using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Configuration.Constants;
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
        private readonly ILessonProgressRepository _lessonProgressRepo;

        public CertificateRepository(ApplicationDbContext context, ILessonProgressRepository lessonProgressRepo)
        {
            _context = context;
            _lessonProgressRepo = lessonProgressRepo;
        }

        public async Task<CertificateResponse> IssueCertificateAsync(string userId, int courseId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("Không tìm thấy học viên.");

            var course = await _context.Courses
                .Include(c => c.Modules.OrderBy(m => m.OrderIndex))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                throw new Exception("Không tìm thấy khóa học.");
            if (!course.IsOnline)
            {
                bool allSessionsCompletedForUser = await _context.ScheduleParticipants
                .Where(sp => sp.UserId == userId && sp.Schedule.CourseId == courseId)
                .AllAsync(sp => sp.Status == ScheduleConstants.ParticipantStatus.Completed);
                if (!allSessionsCompletedForUser)
                    throw new Exception("Học viên chưa hoàn thành toàn bộ buổi học, không thể cấp chứng chỉ.");
            }
            else
            {
                var lessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
                var progressMap = await _lessonProgressRepo.GetProgressMapAsync(userId, lessonIds);

                var totalLessons = lessonIds.Count;
                var completedLessons = progressMap.Values.Count(p => p.IsDone);
                var progressPercent = totalLessons == 0 ? 0 : completedLessons * 100 / totalLessons;
                if (progressPercent < 100)
                {
                    throw new Exception("Học viên chưa hoàn thành toàn bộ khóa học, không thể cấp chứng chỉ.");
                }
            }

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
            var courseEnrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(ce => ce.CourseId == courseId && ce.UserId == userId);
            courseEnrollment!.Status = EnrollmentConstants.Status.Completed;

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
                CourseName = c.Course.CourseName,
                IssueDate = c.IssueDate,
                ExpirationDate = c.ExpirationDate
            })
            .ToListAsync();

            return certificates;
        }
    }
}
