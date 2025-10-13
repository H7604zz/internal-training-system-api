using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseService
    {
        public Course? GetCourseByCourseID(int? couseId);
        Task<IEnumerable<CourseListDto>> GetAllCoursesAsync();
        Task<IEnumerable<CourseListDto>> GetCoursesByIdentifiersAsync(List<string> identifiers);
        Task<CourseDetailDto?> GetCourseDetailAsync(int courseId);
    }
}
