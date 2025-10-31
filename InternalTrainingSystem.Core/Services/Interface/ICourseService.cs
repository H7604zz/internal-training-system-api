using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.Core.Configuration;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseService
    {
        public Course? GetCourseByCourseID(int? couseId);
        Task<PagedResult<CourseListItemDto>> GetAllCoursesPagedAsync(GetAllCoursesRequest request);
        Task<CourseDetailDto?> GetCourseDetailAsync(int courseId, CancellationToken ct = default);
        Task<bool> DeleteCourseAsync(int id);
        Task<Course?> UpdateCourseAsync(UpdateCourseDto dto);
        public bool ToggleStatus(int id, string status);
        Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default);
        Task<bool> UpdatePendingCourseStatusAsync(int courseId, string newStatus);
        Task<bool> DeleteActiveCourseAsync(int courseId, string rejectReason);
        Task<Course> GetCourseByCourseCodeAsync(string courseCode);
        Task<Course> CreateCourseAsync(CreateCourseMetadataDto meta, IList<IFormFile> lessonFiles,
                                            string createdByUserId, CancellationToken ct = default);
    }
}
