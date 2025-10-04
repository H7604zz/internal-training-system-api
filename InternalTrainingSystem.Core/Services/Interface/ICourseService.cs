using InternalTrainingSystem.Core.DTOs.Courses;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.Core.Configuration;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseService
    {
        public List<Course> GetCourses();
        public bool UpdateCourses(Course course);
        public bool DeleteCoursesByCourseId(int id);
        public Course? CreateCourses(Course course);
        public bool ToggleStatus(int id, bool isActive);
        Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default);

    }
}
