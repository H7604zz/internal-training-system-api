using DocumentFormat.OpenXml.InkML;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseEnrollmentService : ICourseEnrollmentService
    {
        private readonly ICourseEnrollmentRepository _courseEnrollmentRepo;

        public CourseEnrollmentService(ICourseEnrollmentRepository courseEnrollmentRepo)
        {
            _courseEnrollmentRepo = courseEnrollmentRepo;
        }
        public async Task<bool> AddCourseEnrollment(CourseEnrollment courseEnrollment)
        {
           return await _courseEnrollmentRepo.AddCourseEnrollment(courseEnrollment);
        }

        public async Task<CourseEnrollment> GetCourseEnrollment(int courseId, string userId)
        {
            return await _courseEnrollmentRepo.GetCourseEnrollment(courseId, userId);
        }
         
        public async Task<bool> DeleteCourseEnrollment(int courseId, string userId)
        {

           return await _courseEnrollmentRepo.DeleteCourseEnrollment(courseId, userId);
        }

        public async Task<bool> UpdateCourseEnrollment(CourseEnrollment courseEnrollment)
        {
           return await _courseEnrollmentRepo.UpdateCourseEnrollment(courseEnrollment);
        }

        public async Task<PagedResult<CourseListItemDto>> GetAllCoursesEnrollmentsByStaffAsync(GetAllCoursesRequest request, string uid)
        {
            return await _courseEnrollmentRepo.GetAllCoursesEnrollmentsByStaffAsync(request, uid);
        }

        public async Task AddRangeAsync(IEnumerable<CourseEnrollment> enrollments)
        {
            await _courseEnrollmentRepo.AddRangeAsync(enrollments);
        }

        public async Task BulkUpdateEnrollmentsToEnrolledAsync(List<EligibleStaffResponse> eligibleUsers, int courseId)
        {
            await _courseEnrollmentRepo.BulkUpdateEnrollmentsToEnrolledAsync(eligibleUsers, courseId);
        }

    }
}
