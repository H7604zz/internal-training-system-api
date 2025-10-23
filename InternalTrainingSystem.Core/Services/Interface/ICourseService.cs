using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.Core.Configuration;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseService
    {
        public Course? GetCourseByCourseID(int? couseId);
        Task<IEnumerable<CourseListDto>> GetAllCoursesAsync();
        Task<IEnumerable<CourseListDto>> GetCoursesByIdentifiersAsync(List<string> identifiers);
        Task<CourseDetailDto?> GetCourseDetailAsync(int courseId);
        Task<bool> DeleteCourseAsync(int id);
        Task<Course?> CreateCourseAsync(Course course, List<int>? departmentIds);
        Task<Course?> UpdateCourseAsync(UpdateCourseDto dto);
        public bool ToggleStatus(int id, string status);
        Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default);
        Task<IEnumerable<CourseListDto>> GetPendingCoursesAsync();
        Task<bool> UpdatePendingCourseStatusAsync(int courseId, string newStatus);
        Task<bool> DeleteActiveCourseAsync(int courseId, string rejectReason);
        Task<Course> GetCourseByCourseCodeAsync(string courseCode);

    }
}
