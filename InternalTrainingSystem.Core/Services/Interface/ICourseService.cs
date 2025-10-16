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
        public bool UpdateCourses(Course course);
        public bool DeleteCoursesByCourseId(int id);
        public Course? CreateCourses(Course course);
        public bool ToggleStatus(int id, string status);
        Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default);

    }
}
